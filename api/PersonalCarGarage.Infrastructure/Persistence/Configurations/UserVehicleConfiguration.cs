using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalCarGarage.Domain.Entities;

namespace PersonalCarGarage.Infrastructure.Persistence.Configurations;

public class UserVehicleConfiguration : IEntityTypeConfiguration<UserVehicle>
{
    public void Configure(EntityTypeBuilder<UserVehicle> builder)
    {
        builder.ToTable("UserVehicles");
        builder.HasKey(x => new { x.UserId, x.VehicleId });

        builder.HasOne(x => x.User)
            .WithMany(x => x.UserVehicles)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Vehicle)
            .WithMany(x => x.UserVehicles)
            .HasForeignKey(x => x.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
