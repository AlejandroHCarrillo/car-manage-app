using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalCarGarage.Domain.Entities;

namespace PersonalCarGarage.Infrastructure.Persistence.Configurations;

public class VehicleDocumentConfiguration : IEntityTypeConfiguration<VehicleDocument>
{
    public void Configure(EntityTypeBuilder<VehicleDocument> builder)
    {
        builder.ToTable("VehicleDocuments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(160).IsRequired();
        builder.Property(x => x.FileName).HasMaxLength(260).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(80).IsRequired();
        builder.Property(x => x.StoragePath).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasOne(x => x.Vehicle)
            .WithMany(x => x.Documents)
            .HasForeignKey(x => x.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.VehicleId);
    }
}
