using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalCarGarage.Domain.Entities;

namespace PersonalCarGarage.Infrastructure.Persistence.Configurations;

public class ProcedureCompletionConfiguration : IEntityTypeConfiguration<ProcedureCompletion>
{
    public void Configure(EntityTypeBuilder<ProcedureCompletion> builder)
    {
        builder.ToTable("ProcedureCompletions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Cost).HasColumnType("decimal(12,2)");
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasOne(x => x.Procedure)
            .WithMany(x => x.Completions)
            .HasForeignKey(x => x.ProcedureId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
