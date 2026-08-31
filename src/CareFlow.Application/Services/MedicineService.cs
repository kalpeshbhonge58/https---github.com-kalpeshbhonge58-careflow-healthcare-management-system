using AutoMapper;
using CareFlow.Application.DTOs.Common;
using CareFlow.Application.Exceptions;
using CareFlow.Application.Interfaces;
using CareFlow.Domain.Entities;
using CareFlow.Shared.Constants;
using CareFlow.Shared.Models;

namespace CareFlow.Application.Services;

public class MedicineService : IMedicineService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    private readonly IMedicineRepository _medicineRepository;
    private readonly IRepository<Medicine> _medicineEntityRepository;
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;

    public MedicineService(
        IMedicineRepository medicineRepository,
        IRepository<Medicine> medicineEntityRepository,
        ICacheService cacheService,
        IMapper mapper)
    {
        _medicineRepository = medicineRepository;
        _medicineEntityRepository = medicineEntityRepository;
        _cacheService = cacheService;
        _mapper = mapper;
    }

    public async Task<PagedResult<MedicineResponse>> GetPagedAsync(PaginationQuery query, CancellationToken cancellationToken = default)
    {
        var paged = await _medicineRepository.GetPagedAsync(query, cancellationToken);
        return new PagedResult<MedicineResponse>
        {
            Items = _mapper.Map<IEnumerable<MedicineResponse>>(paged.Items),
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize,
            TotalRecords = paged.TotalRecords
        };
    }

    public async Task<IReadOnlyList<MedicineResponse>> GetActiveMedicinesAsync(CancellationToken cancellationToken = default)
    {
        var cached = await _cacheService.GetAsync<IReadOnlyList<MedicineResponse>>(CacheKeys.MedicinesLookup, cancellationToken);
        if (cached is not null)
            return cached;

        var medicines = await _medicineRepository.GetActiveMedicinesAsync(cancellationToken);
        var response = _mapper.Map<IReadOnlyList<MedicineResponse>>(medicines);

        await _cacheService.SetAsync(CacheKeys.MedicinesLookup, response, CacheDuration, cancellationToken);
        return response;
    }

    public async Task<MedicineResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var medicine = await _medicineEntityRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Medicine), id);

        return _mapper.Map<MedicineResponse>(medicine);
    }
}
