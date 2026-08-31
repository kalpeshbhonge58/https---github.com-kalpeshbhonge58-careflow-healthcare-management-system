using CareFlow.Domain.Common;

namespace CareFlow.Domain.Entities;

public class Prescription : BaseEntity
{
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
    public int? MedicalRecordId { get; set; }
    public MedicalRecord? MedicalRecord { get; set; }
    public DateTime PrescriptionDate { get; set; }
    public string? Instructions { get; set; }
    public string? Notes { get; set; }

    public ICollection<PrescriptionItem> Items { get; set; } = new List<PrescriptionItem>();
}
