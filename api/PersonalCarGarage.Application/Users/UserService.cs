using PersonalCarGarage.Application.Abstractions;
using PersonalCarGarage.Domain.Entities;
using PersonalCarGarage.Domain.Enums;
using PersonalCarGarage.Domain.Exceptions;
using PersonalCarGarage.Domain.Interfaces;

namespace PersonalCarGarage.Application.Users;

public interface IUserService
{
    Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public class UserService(
    IUserRepository users,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ICurrentUser currentUser) : IUserService
{
    public async Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        var items = await users.GetAllAsync(cancellationToken);
        return items.Select(Map).ToList();
    }

    public async Task<UserResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        var user = await users.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Usuario no encontrado.");
        return Map(user);
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        Validate(request.Email, request.Password, request.FirstName, request.LastName);

        if (await users.EmailExistsAsync(request.Email.Trim(), null, cancellationToken))
        {
            throw new ConflictException("Ya existe un usuario con ese correo.");
        }

        if (!Enum.IsDefined(request.Role))
        {
            throw new AppException("Rol inválido.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHasher.Hash(request.Password),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Role = request.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await users.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(user);
    }

    public async Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        EnsureAdmin();

        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
        {
            throw new AppException("El nombre y el apellido son obligatorios.");
        }

        var user = await users.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Usuario no encontrado.");

        if (id == currentUser.UserId && request.Role != UserRole.Admin)
        {
            throw new AppException("No puedes quitarte el rol de administrador.");
        }

        if (id == currentUser.UserId && !request.IsActive)
        {
            throw new AppException("No puedes desactivar tu propia cuenta.");
        }

        if (!string.IsNullOrWhiteSpace(request.Password) && request.Password.Length < 8)
        {
            throw new AppException("La contraseña debe tener al menos 8 caracteres.");
        }

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.Role = request.Role;
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.PasswordHash = passwordHasher.Hash(request.Password);
        }

        users.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(user);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        EnsureAdmin();

        if (id == currentUser.UserId)
        {
            throw new AppException("No puedes eliminar tu propia cuenta.");
        }

        var user = await users.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Usuario no encontrado.");

        users.Remove(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private void EnsureAdmin()
    {
        if (!currentUser.IsAdmin)
        {
            throw new ForbiddenException("Solo el administrador de la plataforma puede gestionar usuarios.");
        }
    }

    private static void Validate(string email, string password, string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            throw new AppException("El correo no es válido.");
        }

        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
        {
            throw new AppException("La contraseña debe tener al menos 8 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            throw new AppException("El nombre y el apellido son obligatorios.");
        }
    }

    private static UserResponse Map(User user) =>
        new(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role,
            user.IsActive,
            user.CreatedAt,
            user.UserVehicles.Select(uv => uv.VehicleId).ToList());
}
