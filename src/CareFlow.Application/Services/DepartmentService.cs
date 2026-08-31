using AutoMapper;
using CareFlow.Application.DTOs.Common;
using CareFlow.Application.Interfaces;
using CareFlow.Shared.Constants;

namespace CareFlow.Application.Services;

public class DepartmentService : IDepartmentService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(60);

    private readonly IDepartmentRepository _departmentRepository;
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;

    public DepartmentService(
        IDepartmentRepository departmentRepository,
        ICacheService cacheService,
        IMapper mapper)
    {
        _departmentRepository = departmentRepository;
        _cacheService = cacheService;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<DepartmentResponse>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        var cached = await _cacheService.GetAsync<IReadOnlyList<DepartmentResponse>>(CacheKeys.Departments, cancellationToken);
        if (cached is not null)
            return cached;

        var departments = await _departmentRepository.GetAllActiveAsync(cancellationToken);
        var response = _mapper.Map<IReadOnlyList<DepartmentResponse>>(departments);

        await _cacheService.SetAsync(CacheKeys.Departments, response, CacheDuration, cancellationToken);
        return response;
    }
}
