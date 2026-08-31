using CareFlow.Application.DTOs.Appointment;
using CareFlow.Shared.Models;

namespace CareFlow.Application.Interfaces;

public interface IAppointmentService
{
    Task<PagedResult<AppointmentResponse>> GetPagedAsync(PaginationQuery query, int? patientId = null, int? doctorId = null, string? status = null, CancellationToken cancellationToken = default);
    Task<AppointmentResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<AppointmentResponse> CreateAsync(CreateAppointmentRequest request, int? performedByUserId = null, CancellationToken cancellationToken = default);
    Task<AppointmentResponse> UpdateAsync(int id, UpdateAppointmentRequest request, int? performedByUserId = null, CancellationToken cancellationToken = default);
    Task<AppointmentResponse> RescheduleAsync(int id, RescheduleAppointmentRequest request, int? performedByUserId = null, CancellationToken cancellationToken = default);
    Task<AppointmentResponse> ConfirmAsync(int id, int? performedByUserId = null, CancellationToken cancellationToken = default);
    Task<AppointmentResponse> CancelAsync(int id, string cancellationReason, int? performedByUserId = null, CancellationToken cancellationToken = default);
    Task<AppointmentResponse> CompleteAsync(int id, int? performedByUserId = null, CancellationToken cancellationToken = default);
}
