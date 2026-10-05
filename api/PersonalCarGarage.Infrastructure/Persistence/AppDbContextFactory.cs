using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PersonalCarGarage.Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=localhost;Database=PersonalCarGarage;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true");

        return new AppDbContext(optionsBuilder.Options);
    }
}
