using CareFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareFlow.Infrastructure.Data.Configurations;

public class MedicineConfiguration : IEntityTypeConfiguration<Medicine>
{
    public void Configure(EntityTypeBuilder<Medicine> builder)
    {
        builder.ToTable("Medicines");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.GenericName)
            .HasMaxLength(200);

        builder.Property(m => m.Manufacturer)
            .HasMaxLength(200);

        builder.Property(m => m.Description)
            .HasMaxLength(1000);

        builder.Property(m => m.DosageForm)
            .HasMaxLength(100);

        builder.Property(m => m.UnitPrice)
            .HasColumnType("decimal(18,2)");

        builder.HasIndex(m => m.Name)
            .HasDatabaseName("IX_Medicines_Name");

        builder.HasIndex(m => m.IsActive)
            .HasDatabaseName("IX_Medicines_IsActive");
    }
}
