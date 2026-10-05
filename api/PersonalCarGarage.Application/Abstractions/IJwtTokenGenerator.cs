using PersonalCarGarage.Domain.Entities;

namespace PersonalCarGarage.Application.Abstractions;

public interface IJwtTokenGenerator
{
    string Generate(User user);
}
