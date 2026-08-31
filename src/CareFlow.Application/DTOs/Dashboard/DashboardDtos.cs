using CareFlow.Application.DTOs.Appointment;
using CareFlow.Application.DTOs.MedicalRecord;
using CareFlow.Application.DTOs.Prescription;

namespace CareFlow.Application.DTOs.Dashboard;

public class AdminDashboardResponse
{
    public int TotalPatients { get; set; }
    public int TotalDoctors { get; set; }
    public int TodayAppointments { get; set; }
    public int PendingConfirmations { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public decimal PendingPayments { get; set; }
    public IReadOnlyList<DepartmentAppointmentStat> AppointmentsByDepartment { get; set; } = Array.Empty<DepartmentAppointmentStat>();
    public IReadOnlyList<DoctorAppointmentStat> AppointmentsByDoctor { get; set; } = Array.Empty<DoctorAppointmentStat>();
}

public class DoctorDashboardResponse
{
    public int TodayAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int PatientCount { get; set; }
    public IReadOnlyList<AppointmentResponse> UpcomingAppointments { get; set; } = Array.Empty<AppointmentResponse>();
}

public class ReceptionistDashboardResponse
{
    public int TodayAppointments { get; set; }
    public int PendingConfirmations { get; set; }
    public int TotalPatients { get; set; }
    public int TotalDoctors { get; set; }
}

public class PatientDashboardResponse
{
    public IReadOnlyList<AppointmentResponse> UpcomingAppointments { get; set; } = Array.Empty<AppointmentResponse>();
    public IReadOnlyList<AppointmentResponse> RecentAppointments { get; set; } = Array.Empty<AppointmentResponse>();
    public IReadOnlyList<PrescriptionResponse> RecentPrescriptions { get; set; } = Array.Empty<PrescriptionResponse>();
    public IReadOnlyList<MedicalRecordResponse> RecentMedicalRecords { get; set; } = Array.Empty<MedicalRecordResponse>();
}

public class DepartmentAppointmentStat
{
    public string Department { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class DoctorAppointmentStat
{
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public int Count { get; set; }
}
