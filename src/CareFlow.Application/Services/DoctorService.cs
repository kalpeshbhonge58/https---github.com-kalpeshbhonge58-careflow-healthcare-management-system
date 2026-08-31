using AutoMapper;
using CareFlow.Application.DTOs.Doctor;
using CareFlow.Application.Exceptions;
using CareFlow.Application.Interfaces;
using CareFlow.Domain.Entities;
using CareFlow.Shared.Constants;
using CareFlow.Shared.Models;

namespace CareFlow.Application.Services;

public class DoctorService : IDoctorService
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Doctor> _doctorEntityRepository;
    private readonly IRepository<Role> _roleRepository;
    private readonly IRepository<Department> _departmentRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditService _auditService;
    private readonly ICacheService _cacheService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public DoctorService(
        IDoctorRepository doctorRepository,
        IRepository<User> userRepository,
        IRepository<Doctor> doctorEntityRepository,
        IRepository<Role> roleRepository,
        IRepository<Department> departmentRepository,
        IPasswordHasher passwordHasher,
        IAuditService auditService,
        ICacheService cacheService,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _doctorRepository = doctorRepository;
        _userRepository = userRepository;
        _doctorEntityRepository = doctorEntityRepository;
        _roleRepository = roleRepository;
        _departmentRepository = departmentRepository;
        _passwordHasher = passwordHasher;
        _auditService = auditService;
        _cacheService = cacheService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<DoctorResponse>> GetPagedAsync(PaginationQuery query, int? departmentId = null, CancellationToken cancellationToken = default)
    {
        var paged = await _doctorRepository.GetPagedAsync(query, departmentId, cancellationToken);
        return new PagedResult<DoctorResponse>
        {
            Items = _mapper.Map<IEnumerable<DoctorResponse>>(paged.Items),
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize,
            TotalRecords = paged.TotalRecords
        };
    }

    public async Task<DoctorResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var doctor = await _doctorRepository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Doctor), id);

        return _mapper.Map<DoctorResponse>(doctor);
    }

    public async Task<IReadOnlyList<DoctorResponse>> GetActiveDoctorsAsync(CancellationToken cancellationToken = default)
    {
        var cached = await _cacheService.GetAsync<IReadOnlyList<DoctorResponse>>(CacheKeys.DoctorsLookup, cancellationToken);
        if (cached is not null)
            return cached;

        var doctors = await _doctorRepository.GetActiveDoctorsAsync(cancellationToken);
        var response = _mapper.Map<IReadOnlyList<DoctorResponse>>(doctors);
        await _cacheService.SetAsync(CacheKeys.DoctorsLookup, response, TimeSpan.FromMinutes(30), cancellationToken);
        return response;
    }

    public async Task<DoctorResponse> CreateAsync(CreateDoctorRequest request, int? performedByUserId = null, CancellationToken cancellationToken = default)
    {
        if (await _userRepository.ExistsAsync(u => u.Email == request.Email.Trim().ToLowerInvariant(), cancellationToken))
            throw new ConflictException("A user with this email already exists.");

        if (await _departmentRepository.GetByIdAsync(request.DepartmentId, cancellationToken) is null)
            throw new NotFoundException(nameof(Department), request.DepartmentId);

        var doctorRole = (await _roleRepository.FindAsync(r => r.Name == Roles.Doctor, cancellationToken)).FirstOrDefault()
            ?? throw new ValidationException("Doctor role is not configured.");

        var user = _mapper.Map<User>(request);
        user.Email = request.Email.Trim().ToLowerInvariant();
        user.PasswordHash = _passwordHasher.Hash(request.Password);

        user.UserRoles.Add(new UserRole { Role = doctorRole });
        await _userRepository.AddAsync(user, cancellationToken);

        var doctor = _mapper.Map<Doctor>(request);
        doctor.User = user;

        await _doctorEntityRepository.AddAsync(doctor, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync(CacheKeys.DoctorsLookup, cancellationToken);
        await _auditService.LogAsync(performedByUserId, AuditActions.DoctorCreated, nameof(Doctor), doctor.Id, "Doctor created.", cancellationToken: cancellationToken);

        var created = await _doctorRepository.GetByIdWithDetailsAsync(doctor.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Doctor), doctor.Id);

        return _mapper.Map<DoctorResponse>(created);
    }

    public async Task<DoctorResponse> UpdateAsync(int id, UpdateDoctorRequest request, int? performedByUserId = null, CancellationToken cancellationToken = default)
    {
        var doctor = await _doctorRepository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Doctor), id);

        if (await _departmentRepository.GetByIdAsync(request.DepartmentId, cancellationToken) is null)
            throw new NotFoundException(nameof(Department), request.DepartmentId);

        doctor.User.FirstName = request.FirstName.Trim();
        doctor.User.LastName = request.LastName.Trim();
        doctor.User.PhoneNumber = request.PhoneNumber?.Trim();
        doctor.User.UpdatedAt = DateTime.UtcNow;
        doctor.DepartmentId = request.DepartmentId;
        doctor.LicenseNumber = request.LicenseNumber.Trim();
        doctor.Specialization = request.Specialization.Trim();
        doctor.Qualification = request.Qualification?.Trim();
        doctor.ExperienceYears = request.ExperienceYears;
        doctor.ConsultationFee = request.ConsultationFee;
        doctor.AvailableFrom = request.AvailableFrom;
        doctor.AvailableTo = request.AvailableTo;
        doctor.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cacheService.RemoveAsync(CacheKeys.DoctorsLookup, cancellationToken);
        await _auditService.LogAsync(performedByUserId, AuditActions.DoctorUpdated, nameof(Doctor), doctor.Id, "Doctor updated.", cancellationToken: cancellationToken);

        return _mapper.Map<DoctorResponse>(doctor);
    }

    public async Task DeactivateAsync(int id, int? performedByUserId = null, CancellationToken cancellationToken = default)
    {
        var doctor = await _doctorRepository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Doctor), id);

        doctor.IsActive = false;
        doctor.User.IsActive = false;
        doctor.UpdatedAt = DateTime.UtcNow;
        doctor.User.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cacheService.RemoveAsync(CacheKeys.DoctorsLookup, cancellationToken);
        await _auditService.LogAsync(performedByUserId, AuditActions.DoctorUpdated, nameof(Doctor), doctor.Id, "Doctor deactivated.", cancellationToken: cancellationToken);
    }
}
