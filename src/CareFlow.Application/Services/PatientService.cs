using AutoMapper;
using CareFlow.Application.DTOs.Patient;
using CareFlow.Application.Exceptions;
using CareFlow.Application.Interfaces;
using CareFlow.Domain.Entities;
using CareFlow.Shared.Constants;
using CareFlow.Shared.Models;
using FluentValidation;

namespace CareFlow.Application.Services;

public class PatientService : IPatientService
{
    private readonly IPatientRepository _patientRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Patient> _patientEntityRepository;
    private readonly IRepository<Role> _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreatePatientRequest> _createValidator;

    public PatientService(
        IPatientRepository patientRepository,
        IRepository<User> userRepository,
        IRepository<Patient> patientEntityRepository,
        IRepository<Role> roleRepository,
        IPasswordHasher passwordHasher,
        IAuditService auditService,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<CreatePatientRequest> createValidator)
    {
        _patientRepository = patientRepository;
        _userRepository = userRepository;
        _patientEntityRepository = patientEntityRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createValidator = createValidator;
    }

    public async Task<PagedResult<PatientResponse>> GetPagedAsync(PaginationQuery query, CancellationToken cancellationToken = default)
    {
        var paged = await _patientRepository.GetPagedAsync(query, cancellationToken);
        return new PagedResult<PatientResponse>
        {
            Items = _mapper.Map<IEnumerable<PatientResponse>>(paged.Items),
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize,
            TotalRecords = paged.TotalRecords
        };
    }

    public async Task<PatientDetailResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var patient = await _patientRepository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Patient), id);

        var detail = _mapper.Map<PatientDetailResponse>(patient);
        detail.TotalAppointments = patient.Appointments.Count;
        detail.TotalMedicalRecords = patient.MedicalRecords.Count;
        detail.RecentAppointments = _mapper.Map<IReadOnlyList<DTOs.Appointment.AppointmentResponse>>(
            patient.Appointments.OrderByDescending(a => a.AppointmentDate).Take(5));

        return detail;
    }

    public async Task<PatientResponse> CreateAsync(CreatePatientRequest request, int? performedByUserId = null, CancellationToken cancellationToken = default)
    {
        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

        if (await _userRepository.ExistsAsync(u => u.Email == request.Email.Trim().ToLowerInvariant(), cancellationToken))
            throw new ConflictException("A user with this email already exists.");

        var patientRole = (await _roleRepository.FindAsync(r => r.Name == Roles.Patient, cancellationToken)).FirstOrDefault()
            ?? throw new ValidationException("Patient role is not configured.");

        var user = _mapper.Map<User>(request);
        user.Email = request.Email.Trim().ToLowerInvariant();
        user.PasswordHash = _passwordHasher.Hash(request.Password);

        user.UserRoles.Add(new UserRole { Role = patientRole });
        await _userRepository.AddAsync(user, cancellationToken);

        var patient = _mapper.Map<Patient>(request);
        patient.User = user;
        patient.PatientNumber = await GeneratePatientNumberAsync(cancellationToken);

        await _patientEntityRepository.AddAsync(patient, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(performedByUserId, AuditActions.PatientCreated, nameof(Patient), patient.Id, "Patient created.", cancellationToken: cancellationToken);

        var created = await _patientRepository.GetByIdWithDetailsAsync(patient.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Patient), patient.Id);

        return _mapper.Map<PatientResponse>(created);
    }

    public async Task<PatientResponse> UpdateAsync(int id, UpdatePatientRequest request, int? performedByUserId = null, CancellationToken cancellationToken = default)
    {
        var patient = await _patientRepository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Patient), id);

        patient.User.FirstName = request.FirstName.Trim();
        patient.User.LastName = request.LastName.Trim();
        patient.User.PhoneNumber = request.PhoneNumber?.Trim();
        patient.User.UpdatedAt = DateTime.UtcNow;
        patient.DateOfBirth = request.DateOfBirth;
        patient.Gender = request.Gender;
        patient.BloodGroup = request.BloodGroup?.Trim();
        patient.Address = request.Address?.Trim();
        patient.EmergencyContactName = request.EmergencyContactName?.Trim();
        patient.EmergencyContactPhone = request.EmergencyContactPhone?.Trim();
        patient.MedicalHistory = request.MedicalHistory?.Trim();
        patient.Allergies = request.Allergies?.Trim();
        patient.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync(performedByUserId, AuditActions.PatientUpdated, nameof(Patient), patient.Id, "Patient updated.", cancellationToken: cancellationToken);

        return _mapper.Map<PatientResponse>(patient);
    }

    public async Task DeactivateAsync(int id, int? performedByUserId = null, CancellationToken cancellationToken = default)
    {
        var patient = await _patientRepository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Patient), id);

        patient.IsActive = false;
        patient.User.IsActive = false;
        patient.UpdatedAt = DateTime.UtcNow;
        patient.User.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync(performedByUserId, AuditActions.PatientDeactivated, nameof(Patient), patient.Id, "Patient deactivated.", cancellationToken: cancellationToken);
    }

    private async Task<string> GeneratePatientNumberAsync(CancellationToken cancellationToken)
    {
        string patientNumber;
        do
        {
            patientNumber = $"PAT-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
        } while (await _patientRepository.PatientNumberExistsAsync(patientNumber, cancellationToken: cancellationToken));

        return patientNumber;
    }
}
