using PersonalCarGarage.Domain.Enums;

namespace PersonalCarGarage.Application.Users;

public record UserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    UserRole Role,
    bool IsActive,
    DateTime CreatedAt,
    IReadOnlyList<Guid> VehicleIds);

public record CreateUserRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    UserRole Role);

public record UpdateUserRequest(
    string FirstName,
    string LastName,
    UserRole Role,
    bool IsActive,
    string? Password);
