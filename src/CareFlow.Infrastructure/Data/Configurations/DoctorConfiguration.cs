using CareFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareFlow.Infrastructure.Data.Configurations;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.ToTable("Doctors");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.LicenseNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(d => d.Specialization)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.Qualification)
            .HasMaxLength(200);

        builder.Property(d => d.ConsultationFee)
            .HasColumnType("decimal(18,2)");

        builder.HasIndex(d => d.LicenseNumber)
            .IsUnique()
            .HasDatabaseName("IX_Doctors_LicenseNumber");

        builder.HasIndex(d => d.UserId)
            .IsUnique()
            .HasDatabaseName("IX_Doctors_UserId");

        builder.HasIndex(d => d.DepartmentId)
            .HasDatabaseName("IX_Doctors_DepartmentId");

        builder.HasIndex(d => d.IsActive)
            .HasDatabaseName("IX_Doctors_IsActive");

        builder.HasOne(d => d.Department)
            .WithMany(dept => dept.Doctors)
            .HasForeignKey(d => d.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
