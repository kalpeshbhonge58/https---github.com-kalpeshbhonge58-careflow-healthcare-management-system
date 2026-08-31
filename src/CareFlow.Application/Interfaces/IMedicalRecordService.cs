using CareFlow.Application.DTOs.MedicalRecord;
using CareFlow.Shared.Models;

namespace CareFlow.Application.Interfaces;

public interface IMedicalRecordService
{
    Task<PagedResult<MedicalRecordResponse>> GetPagedAsync(PaginationQuery query, int? patientId = null, int? doctorId = null, CancellationToken cancellationToken = default);
    Task<MedicalRecordResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MedicalRecordResponse>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default);
    Task<MedicalRecordResponse> CreateAsync(CreateMedicalRecordRequest request, int? performedByUserId = null, CancellationToken cancellationToken = default);
    Task<MedicalRecordResponse> UpdateAsync(int id, UpdateMedicalRecordRequest request, int? performedByUserId = null, CancellationToken cancellationToken = default);
}
