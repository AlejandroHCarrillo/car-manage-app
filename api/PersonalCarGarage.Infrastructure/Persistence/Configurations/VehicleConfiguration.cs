using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalCarGarage.Domain.Entities;

namespace PersonalCarGarage.Infrastructure.Persistence.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Brand).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Model).HasMaxLength(80).IsRequired();
        builder.Property(x => x.LicensePlate).HasMaxLength(20).IsRequired();
        builder.HasIndex(x => x.LicensePlate).IsUnique();
        builder.Property(x => x.Vin).HasMaxLength(32);
        builder.Property(x => x.Color).HasMaxLength(40).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Ignore(x => x.DisplayName);
    }
}
