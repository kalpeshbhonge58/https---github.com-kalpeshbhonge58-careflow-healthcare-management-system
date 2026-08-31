using CareFlow.Application.DTOs.Prescription;
using CareFlow.Shared.Models;

namespace CareFlow.Application.Interfaces;

public interface IPrescriptionService
{
    Task<PagedResult<PrescriptionResponse>> GetPagedAsync(PaginationQuery query, int? patientId = null, int? doctorId = null, CancellationToken cancellationToken = default);
    Task<PrescriptionResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PrescriptionResponse> CreateAsync(CreatePrescriptionRequest request, int? performedByUserId = null, CancellationToken cancellationToken = default);
}
