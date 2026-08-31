using CareFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareFlow.Infrastructure.Data.Configurations;

public class PrescriptionItemConfiguration : IEntityTypeConfiguration<PrescriptionItem>
{
    public void Configure(EntityTypeBuilder<PrescriptionItem> builder)
    {
        builder.ToTable("PrescriptionItems");

        builder.HasKey(pi => pi.Id);

        builder.Property(pi => pi.Dosage)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(pi => pi.Frequency)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(pi => pi.Instructions)
            .HasMaxLength(500);

        builder.HasIndex(pi => pi.PrescriptionId)
            .HasDatabaseName("IX_PrescriptionItems_PrescriptionId");

        builder.HasIndex(pi => pi.MedicineId)
            .HasDatabaseName("IX_PrescriptionItems_MedicineId");

        builder.HasOne(pi => pi.Medicine)
            .WithMany(m => m.PrescriptionItems)
            .HasForeignKey(pi => pi.MedicineId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
