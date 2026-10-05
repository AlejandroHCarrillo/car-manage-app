using PersonalCarGarage.Application.Abstractions;
using PersonalCarGarage.Domain.Exceptions;
using PersonalCarGarage.Domain.Interfaces;

namespace PersonalCarGarage.Application.Auth;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<CurrentUserResponse> GetCurrentAsync(CancellationToken cancellationToken = default);
}

public class AuthService(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator,
    ICurrentUser currentUser) : IAuthService
{
    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new AppException("El correo y la contraseña son obligatorios.");
        }

        var user = await users.GetByEmailAsync(request.Email.Trim(), cancellationToken)
            ?? throw new UnauthorizedException("Credenciales inválidas.");

        if (!user.IsActive)
        {
            throw new ForbiddenException("La cuenta está desactivada.");
        }

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Credenciales inválidas.");
        }

        return new AuthResponse(
            jwtTokenGenerator.Generate(user),
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role);
    }

    public async Task<CurrentUserResponse> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated)
        {
            throw new UnauthorizedException("No autenticado.");
        }

        var user = await users.GetByIdAsync(currentUser.UserId, cancellationToken)
            ?? throw new UnauthorizedException("Usuario no encontrado.");

        return new CurrentUserResponse(user.Id, user.Email, user.FirstName, user.LastName, user.Role, user.IsActive);
    }
}
