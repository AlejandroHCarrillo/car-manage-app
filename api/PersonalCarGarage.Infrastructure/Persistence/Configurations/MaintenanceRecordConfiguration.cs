using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalCarGarage.Domain.Entities;

namespace PersonalCarGarage.Infrastructure.Persistence.Configurations;

public class MaintenanceRecordConfiguration : IEntityTypeConfiguration<MaintenanceRecord>
{
    public void Configure(EntityTypeBuilder<MaintenanceRecord> builder)
    {
        builder.ToTable("MaintenanceRecords");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.Cost).HasColumnType("decimal(12,2)");

        builder.HasOne(x => x.Vehicle)
            .WithMany(x => x.MaintenanceRecords)
            .HasForeignKey(x => x.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.CreatedByUser)
            .WithMany(x => x.MaintenanceRecords)
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
