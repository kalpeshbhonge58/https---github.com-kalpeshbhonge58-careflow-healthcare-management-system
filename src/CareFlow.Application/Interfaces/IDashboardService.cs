using CareFlow.Application.DTOs.Dashboard;

namespace CareFlow.Application.Interfaces;

public interface IDashboardService
{
    Task<AdminDashboardResponse> GetAdminDashboardAsync(CancellationToken cancellationToken = default);
    Task<DoctorDashboardResponse> GetDoctorDashboardAsync(int doctorId, CancellationToken cancellationToken = default);
    Task<ReceptionistDashboardResponse> GetReceptionistDashboardAsync(CancellationToken cancellationToken = default);
    Task<PatientDashboardResponse> GetPatientDashboardAsync(int patientId, CancellationToken cancellationToken = default);
}
