using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalCarGarage.Domain.Entities;

namespace PersonalCarGarage.Infrastructure.Persistence.Configurations;

public class VehiclePhotoConfiguration : IEntityTypeConfiguration<VehiclePhoto>
{
    public void Configure(EntityTypeBuilder<VehiclePhoto> builder)
    {
        builder.ToTable("VehiclePhotos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FileName).HasMaxLength(260).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(80).IsRequired();
        builder.Property(x => x.StoragePath).HasMaxLength(500).IsRequired();

        builder.HasOne(x => x.Vehicle)
            .WithMany(x => x.Photos)
            .HasForeignKey(x => x.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
