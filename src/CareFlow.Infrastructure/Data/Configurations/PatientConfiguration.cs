using CareFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareFlow.Infrastructure.Data.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.PatientNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(p => p.BloodGroup)
            .HasMaxLength(10);

        builder.Property(p => p.Address)
            .HasMaxLength(500);

        builder.Property(p => p.EmergencyContactName)
            .HasMaxLength(200);

        builder.Property(p => p.EmergencyContactPhone)
            .HasMaxLength(20);

        builder.Property(p => p.MedicalHistory)
            .HasMaxLength(2000);

        builder.Property(p => p.Allergies)
            .HasMaxLength(1000);

        builder.Property(p => p.Gender)
            .HasConversion<int>();

        builder.HasIndex(p => p.PatientNumber)
            .IsUnique()
            .HasDatabaseName("IX_Patients_PatientNumber");

        builder.HasIndex(p => p.UserId)
            .IsUnique()
            .HasDatabaseName("IX_Patients_UserId");

        builder.HasIndex(p => p.IsActive)
            .HasDatabaseName("IX_Patients_IsActive");
    }
}
