using PersonalCarGarage.Application.Abstractions;
using PersonalCarGarage.Domain.Entities;
using PersonalCarGarage.Domain.Enums;
using PersonalCarGarage.Domain.Exceptions;
using PersonalCarGarage.Domain.Interfaces;

namespace PersonalCarGarage.Application.Vehicles;

public interface IVehicleService
{
    Task<IReadOnlyList<VehicleResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<VehicleResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<VehicleResponse> CreateAsync(UpsertVehicleRequest request, CancellationToken cancellationToken = default);
    Task<VehicleResponse> UpdateAsync(Guid id, UpsertVehicleRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task AssignManagerAsync(Guid vehicleId, Guid userId, CancellationToken cancellationToken = default);
    Task UnassignManagerAsync(Guid vehicleId, Guid userId, CancellationToken cancellationToken = default);
    Task<DashboardStatsResponse> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<VehiclePhotoResponse> AddPhotoAsync(Guid vehicleId, string fileName, string contentType, Stream content, CancellationToken cancellationToken = default);
    Task DeletePhotoAsync(Guid vehicleId, Guid photoId, CancellationToken cancellationToken = default);
    Task<VehicleDocumentResponse> AddDocumentAsync(
        Guid vehicleId,
        string title,
        DocumentKind kind,
        string fileName,
        string contentType,
        Stream content,
        string? notes,
        DateTime? issuedOn,
        DateTime? expiresOn,
        CancellationToken cancellationToken = default);
    Task DeleteDocumentAsync(Guid vehicleId, Guid documentId, CancellationToken cancellationToken = default);
}

public class VehicleService(
    IVehicleRepository vehicles,
    IUserRepository users,
    IMaintenanceRepository maintenance,
    IProcedureRepository procedures,
    IPhotoStorage photoStorage,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser) : IVehicleService
{
    public async Task<IReadOnlyList<VehicleResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = currentUser.IsAdmin
            ? await vehicles.GetAllAsync(cancellationToken)
            : await vehicles.GetByUserIdAsync(currentUser.UserId, cancellationToken);

        return items.Select(Map).ToList();
    }

    public async Task<VehicleResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var vehicle = await GetAccessibleVehicleAsync(id, cancellationToken);
        return Map(vehicle);
    }

    public async Task<VehicleResponse> CreateAsync(UpsertVehicleRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        if (await vehicles.LicensePlateExistsAsync(request.LicensePlate.Trim(), null, cancellationToken))
        {
            throw new ConflictException("Ya existe un vehículo con esa placa.");
        }

        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = request.Brand.Trim(),
            Model = request.Model.Trim(),
            Year = request.Year,
            LicensePlate = request.LicensePlate.Trim().ToUpperInvariant(),
            Vin = string.IsNullOrWhiteSpace(request.Vin) ? null : request.Vin.Trim().ToUpperInvariant(),
            Color = request.Color.Trim(),
            CurrentMileage = request.CurrentMileage,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await vehicles.AddAsync(vehicle, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (!currentUser.IsAdmin)
        {
            await vehicles.AssignUserAsync(vehicle.Id, currentUser.UserId, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var created = await vehicles.GetByIdAsync(vehicle.Id, cancellationToken) ?? vehicle;
        return Map(created);
    }

    public async Task<VehicleResponse> UpdateAsync(Guid id, UpsertVehicleRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);
        var vehicle = await GetAccessibleVehicleAsync(id, cancellationToken);

        if (await vehicles.LicensePlateExistsAsync(request.LicensePlate.Trim(), id, cancellationToken))
        {
            throw new ConflictException("Ya existe un vehículo con esa placa.");
        }

        vehicle.Brand = request.Brand.Trim();
        vehicle.Model = request.Model.Trim();
        vehicle.Year = request.Year;
        vehicle.LicensePlate = request.LicensePlate.Trim().ToUpperInvariant();
        vehicle.Vin = string.IsNullOrWhiteSpace(request.Vin) ? null : request.Vin.Trim().ToUpperInvariant();
        vehicle.Color = request.Color.Trim();
        vehicle.CurrentMileage = request.CurrentMileage;
        vehicle.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        vehicle.UpdatedAt = DateTime.UtcNow;

        vehicles.Update(vehicle);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await vehicles.GetByIdAsync(id, cancellationToken) ?? vehicle;
        return Map(updated);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            throw new ForbiddenException("Solo el administrador puede eliminar vehículos.");
        }

        var vehicle = await vehicles.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Vehículo no encontrado.");

        foreach (var photo in vehicle.Photos)
        {
            photoStorage.Delete(photo.StoragePath);
        }

        foreach (var document in vehicle.Documents)
        {
            photoStorage.Delete(document.StoragePath);
        }

        vehicles.Remove(vehicle);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task AssignManagerAsync(Guid vehicleId, Guid userId, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            throw new ForbiddenException("Solo el administrador puede asignar gestores.");
        }

        var vehicle = await vehicles.GetByIdAsync(vehicleId, cancellationToken)
            ?? throw new NotFoundException("Vehículo no encontrado.");

        var user = await users.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Usuario no encontrado.");

        if (user.Role != UserRole.VehicleManager)
        {
            throw new AppException("Solo se pueden asignar usuarios con rol de gestor de vehículos.");
        }

        if (await vehicles.IsAssignedToUserAsync(vehicle.Id, user.Id, cancellationToken))
        {
            throw new ConflictException("El usuario ya está asignado a este vehículo.");
        }

        await vehicles.AssignUserAsync(vehicle.Id, user.Id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UnassignManagerAsync(Guid vehicleId, Guid userId, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            throw new ForbiddenException("Solo el administrador puede quitar gestores.");
        }

        _ = await vehicles.GetByIdAsync(vehicleId, cancellationToken)
            ?? throw new NotFoundException("Vehículo no encontrado.");

        if (!await vehicles.IsAssignedToUserAsync(vehicleId, userId, cancellationToken))
        {
            throw new NotFoundException("El usuario no está asignado a este vehículo.");
        }

        await vehicles.UnassignUserAsync(vehicleId, userId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<DashboardStatsResponse> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var items = currentUser.IsAdmin
            ? await vehicles.GetAllAsync(cancellationToken)
            : await vehicles.GetByUserIdAsync(currentUser.UserId, cancellationToken);

        var records = new List<MaintenanceRecord>();
        foreach (var vehicle in items)
        {
            records.AddRange(await maintenance.GetByVehicleIdAsync(vehicle.Id, cancellationToken));
        }

        var userCount = currentUser.IsAdmin
            ? (await users.GetAllAsync(cancellationToken)).Count
            : items.SelectMany(v => v.UserVehicles).Select(uv => uv.UserId).Distinct().Count();

        var procedureItems = await procedures.GetByVehicleIdsAsync(items.Select(x => x.Id).ToList(), cancellationToken);
        var today = DateTime.UtcNow.Date;
        var pending = procedureItems.Count(x => !x.IsCompleted);
        var overdue = procedureItems.Count(x => !x.IsCompleted && x.DueDate.Date < today);

        return new DashboardStatsResponse(
            items.Count,
            userCount,
            records.Count,
            records.Sum(r => r.Cost),
            pending,
            overdue);
    }

    public async Task<VehiclePhotoResponse> AddPhotoAsync(Guid vehicleId, string fileName, string contentType, Stream content, CancellationToken cancellationToken = default)
    {
        var vehicle = await GetAccessibleVehicleAsync(vehicleId, cancellationToken);

        if (vehicle.Photos.Count >= 8)
        {
            throw new AppException("Puedes agregar un máximo de 8 fotos por vehículo.");
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };
        if (!allowed.Contains(extension) || !contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            throw new AppException("Solo se permiten imágenes JPG, PNG o WEBP.");
        }

        var photoId = Guid.NewGuid();
        var storagePath = await photoStorage.SaveAsync(vehicle.Id, photoId, extension, content, cancellationToken);

        var photo = new VehiclePhoto
        {
            Id = photoId,
            VehicleId = vehicle.Id,
            FileName = Path.GetFileName(fileName),
            ContentType = contentType,
            StoragePath = storagePath,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            vehicles.AddPhoto(photo);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            photoStorage.Delete(storagePath);
            throw;
        }

        return new VehiclePhotoResponse(photo.Id, photoStorage.GetPublicUrl(photo.StoragePath), photo.ContentType);
    }

    public async Task DeletePhotoAsync(Guid vehicleId, Guid photoId, CancellationToken cancellationToken = default)
    {
        var vehicle = await GetAccessibleVehicleAsync(vehicleId, cancellationToken);
        var photo = vehicle.Photos.FirstOrDefault(x => x.Id == photoId)
            ?? throw new NotFoundException("Foto no encontrada.");

        photoStorage.Delete(photo.StoragePath);
        vehicle.Photos.Remove(photo);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<VehicleDocumentResponse> AddDocumentAsync(
        Guid vehicleId,
        string title,
        DocumentKind kind,
        string fileName,
        string contentType,
        Stream content,
        string? notes,
        DateTime? issuedOn,
        DateTime? expiresOn,
        CancellationToken cancellationToken = default)
    {
        var vehicle = await GetAccessibleVehicleAsync(vehicleId, cancellationToken);

        if (vehicle.Documents.Count >= 20)
        {
            throw new AppException("Puedes agregar un máximo de 20 documentos por vehículo.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new AppException("El título del documento es obligatorio.");
        }

        if (!Enum.IsDefined(kind))
        {
            throw new AppException("El tipo de documento no es válido.");
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".pdf", ".jpg", ".jpeg", ".png", ".webp" };
        if (!allowed.Contains(extension))
        {
            throw new AppException("Solo se permiten PDF, JPG, PNG o WEBP.");
        }

        var isPdf = extension == ".pdf" || contentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase);
        var isImage = contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
        if (!isPdf && !isImage)
        {
            throw new AppException("El archivo debe ser un PDF o una imagen.");
        }

        var documentId = Guid.NewGuid();
        var storagePath = await photoStorage.SaveInFolderAsync(vehicle.Id, documentId, extension, "documents", content, cancellationToken);

        var document = new VehicleDocument
        {
            Id = documentId,
            VehicleId = vehicle.Id,
            Title = title.Trim(),
            Kind = kind,
            FileName = Path.GetFileName(fileName),
            ContentType = contentType,
            StoragePath = storagePath,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            IssuedOn = issuedOn,
            ExpiresOn = expiresOn,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            vehicles.AddDocument(document);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            photoStorage.Delete(storagePath);
            throw;
        }

        return MapDocument(document);
    }

    public async Task DeleteDocumentAsync(Guid vehicleId, Guid documentId, CancellationToken cancellationToken = default)
    {
        var vehicle = await GetAccessibleVehicleAsync(vehicleId, cancellationToken);
        var document = vehicle.Documents.FirstOrDefault(x => x.Id == documentId)
            ?? throw new NotFoundException("Documento no encontrado.");

        photoStorage.Delete(document.StoragePath);
        vehicle.Documents.Remove(document);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Vehicle> GetAccessibleVehicleAsync(Guid id, CancellationToken cancellationToken)
    {
        var vehicle = await vehicles.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Vehículo no encontrado.");

        if (!currentUser.IsAdmin &&
            !await vehicles.IsAssignedToUserAsync(id, currentUser.UserId, cancellationToken))
        {
            throw new ForbiddenException("No tienes acceso a este vehículo.");
        }

        return vehicle;
    }

    private static void Validate(UpsertVehicleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Brand) || string.IsNullOrWhiteSpace(request.Model))
        {
            throw new AppException("La marca y el modelo son obligatorios.");
        }

        if (request.Year < 1950 || request.Year > DateTime.UtcNow.Year + 1)
        {
            throw new AppException("El año del vehículo no es válido.");
        }

        if (string.IsNullOrWhiteSpace(request.LicensePlate))
        {
            throw new AppException("La placa es obligatoria.");
        }

        if (request.CurrentMileage < 0)
        {
            throw new AppException("El kilometraje no puede ser negativo.");
        }
    }

    private static VehicleResponse Map(Vehicle vehicle) =>
        new(
            vehicle.Id,
            vehicle.Brand,
            vehicle.Model,
            vehicle.Year,
            vehicle.LicensePlate,
            vehicle.Vin,
            vehicle.Color,
            vehicle.CurrentMileage,
            vehicle.Notes,
            vehicle.CreatedAt,
            vehicle.UpdatedAt,
            vehicle.UserVehicles
                .Where(uv => uv.User is not null)
                .Select(uv => new VehicleManagerResponse(uv.User.Id, uv.User.Email, uv.User.FullName))
                .ToList(),
            vehicle.Photos
                .OrderBy(x => x.CreatedAt)
                .Select(x => new VehiclePhotoResponse(x.Id, "/" + x.StoragePath.TrimStart('/'), x.ContentType))
                .ToList(),
            vehicle.Documents
                .OrderByDescending(x => x.CreatedAt)
                .Select(MapDocument)
                .ToList());

    private static VehicleDocumentResponse MapDocument(VehicleDocument document) =>
        new(
            document.Id,
            document.Title,
            document.Kind,
            document.FileName,
            "/" + document.StoragePath.TrimStart('/'),
            document.ContentType,
            document.Notes,
            document.IssuedOn,
            document.ExpiresOn,
            document.CreatedAt);
}
