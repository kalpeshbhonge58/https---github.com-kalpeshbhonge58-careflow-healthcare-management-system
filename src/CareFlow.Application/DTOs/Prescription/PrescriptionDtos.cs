namespace CareFlow.Application.DTOs.Prescription;

public class CreatePrescriptionRequest
{
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public int? MedicalRecordId { get; set; }
    public DateTime PrescriptionDate { get; set; }
    public string? Instructions { get; set; }
    public string? Notes { get; set; }
    public List<PrescriptionItemRequest> Items { get; set; } = new();
}

public class PrescriptionItemRequest
{
    public int MedicineId { get; set; }
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public int DurationDays { get; set; }
    public string? Instructions { get; set; }
    public int Quantity { get; set; }
}

public class PrescriptionResponse
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public int? MedicalRecordId { get; set; }
    public DateTime PrescriptionDate { get; set; }
    public string? Instructions { get; set; }
    public string? Notes { get; set; }
    public IReadOnlyList<PrescriptionItemResponse> Items { get; set; } = Array.Empty<PrescriptionItemResponse>();
    public DateTime CreatedAt { get; set; }
}

public class PrescriptionItemResponse
{
    public int Id { get; set; }
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public int DurationDays { get; set; }
    public string? Instructions { get; set; }
    public int Quantity { get; set; }
}
