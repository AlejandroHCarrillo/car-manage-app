using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using PersonalCarGarage.Application.Abstractions;
using PersonalCarGarage.Domain.Enums;

namespace PersonalCarGarage.Api.Security;

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid UserId
    {
        get
        {
            var value = Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub);

            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }

    public string Email =>
        Principal?.FindFirstValue(ClaimTypes.Email)
        ?? Principal?.FindFirstValue(JwtRegisteredClaimNames.Email)
        ?? string.Empty;

    public UserRole Role =>
        Enum.TryParse<UserRole>(Principal?.FindFirstValue(ClaimTypes.Role), out var role)
            ? role
            : default;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public bool IsAdmin => Role == UserRole.Admin;

    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;
}
