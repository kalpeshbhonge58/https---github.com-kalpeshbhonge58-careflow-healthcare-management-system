using CareFlow.Application.DTOs.Doctor;
using CareFlow.Shared.Models;

namespace CareFlow.Application.Interfaces;

public interface IDoctorService
{
    Task<PagedResult<DoctorResponse>> GetPagedAsync(PaginationQuery query, int? departmentId = null, CancellationToken cancellationToken = default);
    Task<DoctorResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DoctorResponse>> GetActiveDoctorsAsync(CancellationToken cancellationToken = default);
    Task<DoctorResponse> CreateAsync(CreateDoctorRequest request, int? performedByUserId = null, CancellationToken cancellationToken = default);
    Task<DoctorResponse> UpdateAsync(int id, UpdateDoctorRequest request, int? performedByUserId = null, CancellationToken cancellationToken = default);
    Task DeactivateAsync(int id, int? performedByUserId = null, CancellationToken cancellationToken = default);
}
