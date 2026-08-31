using CareFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareFlow.Infrastructure.Data.Configurations;

public class MedicalRecordConfiguration : IEntityTypeConfiguration<MedicalRecord>
{
    public void Configure(EntityTypeBuilder<MedicalRecord> builder)
    {
        builder.ToTable("MedicalRecords");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Diagnosis)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(m => m.Symptoms)
            .HasMaxLength(2000);

        builder.Property(m => m.Treatment)
            .HasMaxLength(2000);

        builder.Property(m => m.Notes)
            .HasMaxLength(2000);

        builder.Property(m => m.DoctorComments)
            .HasMaxLength(2000);

        builder.HasIndex(m => m.PatientId)
            .HasDatabaseName("IX_MedicalRecords_PatientId");

        builder.HasIndex(m => m.DoctorId)
            .HasDatabaseName("IX_MedicalRecords_DoctorId");

        builder.HasIndex(m => m.AppointmentId)
            .IsUnique()
            .HasFilter("[AppointmentId] IS NOT NULL")
            .HasDatabaseName("IX_MedicalRecords_AppointmentId");

        builder.HasOne(m => m.Patient)
            .WithMany(p => p.MedicalRecords)
            .HasForeignKey(m => m.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Doctor)
            .WithMany(d => d.MedicalRecords)
            .HasForeignKey(m => m.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
