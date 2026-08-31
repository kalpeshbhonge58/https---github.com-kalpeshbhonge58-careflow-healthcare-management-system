using CareFlow.Domain.Common;
using CareFlow.Domain.Enums;

namespace CareFlow.Domain.Entities;

public class Patient : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string PatientNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string? BloodGroup { get; set; }
    public string? Address { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? MedicalHistory { get; set; }
    public string? Allergies { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
