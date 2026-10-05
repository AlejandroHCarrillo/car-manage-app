using PersonalCarGarage.Domain.Enums;

namespace PersonalCarGarage.Application.Auth;

public record LoginRequest(string Email, string Password);

public record AuthResponse(
    string Token,
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    UserRole Role);

public record CurrentUserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    UserRole Role,
    bool IsActive);
