using CareFlow.Application.DTOs.Patient;
using CareFlow.Shared.Models;

namespace CareFlow.Application.Interfaces;

public interface IPatientService
{
    Task<PagedResult<PatientResponse>> GetPagedAsync(PaginationQuery query, CancellationToken cancellationToken = default);
    Task<PatientDetailResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PatientResponse> CreateAsync(CreatePatientRequest request, int? performedByUserId = null, CancellationToken cancellationToken = default);
    Task<PatientResponse> UpdateAsync(int id, UpdatePatientRequest request, int? performedByUserId = null, CancellationToken cancellationToken = default);
    Task DeactivateAsync(int id, int? performedByUserId = null, CancellationToken cancellationToken = default);
}
