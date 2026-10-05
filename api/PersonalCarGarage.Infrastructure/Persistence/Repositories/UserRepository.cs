using Microsoft.EntityFrameworkCore;
using PersonalCarGarage.Domain.Entities;
using PersonalCarGarage.Domain.Interfaces;

namespace PersonalCarGarage.Infrastructure.Persistence.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Users
            .Include(x => x.UserVehicles)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        db.Users.FirstOrDefaultAsync(x => x.Email == email.Trim().ToLowerInvariant(), cancellationToken);

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await db.Users
            .Include(x => x.UserVehicles)
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .ToListAsync(cancellationToken);

    public Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return db.Users.AnyAsync(
            x => x.Email == normalized && (!excludeUserId.HasValue || x.Id != excludeUserId.Value),
            cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default) =>
        await db.Users.AddAsync(user, cancellationToken);

    public void Update(User user) => db.Users.Update(user);

    public void Remove(User user) => db.Users.Remove(user);
}
