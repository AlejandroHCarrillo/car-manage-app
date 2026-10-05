using PersonalCarGarage.Domain.Enums;

namespace PersonalCarGarage.Application.Abstractions;

public interface ICurrentUser
{
    Guid UserId { get; }
    string Email { get; }
    UserRole Role { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
}
