using CareFlow.Application.DTOs.Common;

namespace CareFlow.Application.Interfaces;

public interface IDepartmentService
{
    Task<IReadOnlyList<DepartmentResponse>> GetAllActiveAsync(CancellationToken cancellationToken = default);
}
