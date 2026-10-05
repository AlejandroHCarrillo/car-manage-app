namespace PersonalCarGarage.Domain.Exceptions;

public class AppException : Exception
{
    public int StatusCode { get; }

    public AppException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }
}

public class NotFoundException(string message) : AppException(message, 404);

public class ConflictException(string message) : AppException(message, 409);

public class ForbiddenException(string message) : AppException(message, 403);

public class UnauthorizedException(string message) : AppException(message, 401);
