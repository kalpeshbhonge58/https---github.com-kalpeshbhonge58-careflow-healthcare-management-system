using CareFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareFlow.Infrastructure.Data.Configurations;

public class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.ToTable("InvoiceItems");

        builder.HasKey(ii => ii.Id);

        builder.Property(ii => ii.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(ii => ii.UnitPrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(ii => ii.TotalPrice)
            .HasColumnType("decimal(18,2)");

        builder.HasIndex(ii => ii.InvoiceId)
            .HasDatabaseName("IX_InvoiceItems_InvoiceId");
    }
}
