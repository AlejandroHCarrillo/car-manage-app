using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalCarGarage.Domain.Entities;

namespace PersonalCarGarage.Infrastructure.Persistence.Configurations;

public class VehicleProcedureConfiguration : IEntityTypeConfiguration<VehicleProcedure>
{
    public void Configure(EntityTypeBuilder<VehicleProcedure> builder)
    {
        builder.ToTable("VehicleProcedures");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.Frequency).HasConversion<int>().IsRequired();
        builder.Property(x => x.LastCost).HasColumnType("decimal(12,2)");
        builder.Ignore(x => x.IsPeriodic);

        builder.HasOne(x => x.Vehicle)
            .WithMany(x => x.Procedures)
            .HasForeignKey(x => x.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
