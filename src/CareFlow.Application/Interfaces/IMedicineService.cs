using CareFlow.Application.DTOs.Common;
using CareFlow.Shared.Models;

namespace CareFlow.Application.Interfaces;

public interface IMedicineService
{
    Task<PagedResult<MedicineResponse>> GetPagedAsync(PaginationQuery query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MedicineResponse>> GetActiveMedicinesAsync(CancellationToken cancellationToken = default);
    Task<MedicineResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
