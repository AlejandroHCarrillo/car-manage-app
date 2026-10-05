using PersonalCarGarage.Domain.Enums;

namespace PersonalCarGarage.Application.Vehicles;

public record VehicleManagerResponse(Guid Id, string Email, string FullName);

public record VehiclePhotoResponse(Guid Id, string Url, string ContentType);

public record VehicleDocumentResponse(
    Guid Id,
    string Title,
    DocumentKind Kind,
    string FileName,
    string Url,
    string ContentType,
    string? Notes,
    DateTime? IssuedOn,
    DateTime? ExpiresOn,
    DateTime CreatedAt);

public record VehicleResponse(
    Guid Id,
    string Brand,
    string Model,
    int Year,
    string LicensePlate,
    string? Vin,
    string Color,
    int CurrentMileage,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<VehicleManagerResponse> Managers,
    IReadOnlyList<VehiclePhotoResponse> Photos,
    IReadOnlyList<VehicleDocumentResponse> Documents);

public record UpsertVehicleRequest(
    string Brand,
    string Model,
    int Year,
    string LicensePlate,
    string? Vin,
    string Color,
    int CurrentMileage,
    string? Notes);

public record AssignManagerRequest(Guid UserId);

public record DashboardStatsResponse(
    int TotalVehicles,
    int TotalUsers,
    int TotalMaintenanceRecords,
    decimal TotalMaintenanceCost,
    int PendingProcedures,
    int OverdueProcedures);
