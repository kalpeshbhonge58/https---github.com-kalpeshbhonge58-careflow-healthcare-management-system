using AutoMapper;
using CareFlow.Application.DTOs.Appointment;
using CareFlow.Application.Exceptions;
using CareFlow.Application.Interfaces;
using CareFlow.Domain.Entities;
using CareFlow.Domain.Enums;
using CareFlow.Shared.Constants;
using CareFlow.Shared.Models;
using FluentValidation;

namespace CareFlow.Application.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IRepository<Appointment> _appointmentEntityRepository;
    private readonly IRepository<Patient> _patientRepository;
    private readonly IRepository<Doctor> _doctorRepository;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateAppointmentRequest> _createValidator;

    private static readonly HashSet<AppointmentStatus> ActiveStatuses =
    [
        AppointmentStatus.Scheduled,
        AppointmentStatus.Confirmed
    ];

    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        IRepository<Appointment> appointmentEntityRepository,
        IRepository<Patient> patientRepository,
        IRepository<Doctor> doctorRepository,
        IAuditService auditService,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<CreateAppointmentRequest> createValidator)
    {
        _appointmentRepository = appointmentRepository;
        _appointmentEntityRepository = appointmentEntityRepository;
        _patientRepository = patientRepository;
        _doctorRepository = doctorRepository;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createValidator = createValidator;
    }

    public async Task<PagedResult<AppointmentResponse>> GetPagedAsync(PaginationQuery query, int? patientId = null, int? doctorId = null, string? status = null, CancellationToken cancellationToken = default)
    {
        var paged = await _appointmentRepository.GetPagedAsync(query, patientId, doctorId, status, cancellationToken);
        return new PagedResult<AppointmentResponse>
        {
            Items = _mapper.Map<IEnumerable<AppointmentResponse>>(paged.Items),
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize,
            TotalRecords = paged.TotalRecords
        };
    }

    public async Task<AppointmentResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var appointment = await _appointmentRepository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Appointment), id);

        return _mapper.Map<AppointmentResponse>(appointment);
    }

    public async Task<AppointmentResponse> CreateAsync(CreateAppointmentRequest request, int? performedByUserId = null, CancellationToken cancellationToken = default)
    {
        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

        await ValidateEntitiesExistAsync(request.PatientId, request.DoctorId, cancellationToken);
        ValidateFutureDateTime(request.AppointmentDate, request.StartTime);
        ValidateTimeRange(request.StartTime, request.EndTime);

        if (await _appointmentRepository.HasDoctorConflictAsync(request.DoctorId, request.AppointmentDate.Date, request.StartTime, request.EndTime, cancellationToken: cancellationToken))
            throw new ConflictException("The selected doctor is already booked for this time.");

        if (await _appointmentRepository.HasPatientConflictAsync(request.PatientId, request.AppointmentDate.Date, request.StartTime, request.EndTime, cancellationToken: cancellationToken))
            throw new ConflictException("The patient already has an overlapping appointment.");

        var appointment = new Appointment
        {
            PatientId = request.PatientId,
            DoctorId = request.DoctorId,
            AppointmentDate = request.AppointmentDate.Date,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Reason = request.Reason?.Trim(),
            Notes = request.Notes?.Trim(),
            Status = AppointmentStatus.Scheduled
        };

        await _appointmentEntityRepository.AddAsync(appointment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync(performedByUserId, AuditActions.AppointmentBooked, nameof(Appointment), appointment.Id, "Appointment booked.", cancellationToken: cancellationToken);

        var created = await _appointmentRepository.GetByIdWithDetailsAsync(appointment.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Appointment), appointment.Id);

        return _mapper.Map<AppointmentResponse>(created);
    }

    public async Task<AppointmentResponse> UpdateAsync(int id, UpdateAppointmentRequest request, int? performedByUserId = null, CancellationToken cancellationToken = default)
    {
        var appointment = await _appointmentRepository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Appointment), id);

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (!Enum.TryParse<AppointmentStatus>(request.Status, true, out var newStatus))
                throw new ValidationException($"Invalid appointment status: {request.Status}.");

            ApplyStatusTransition(appointment, newStatus, request.CancellationReason);
        }

        if (request.Reason is not null)
            appointment.Reason = request.Reason.Trim();

        if (request.Notes is not null)
            appointment.Notes = request.Notes.Trim();

        appointment.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<AppointmentResponse>(appointment);
    }

    public async Task<AppointmentResponse> RescheduleAsync(int id, RescheduleAppointmentRequest request, int? performedByUserId = null, CancellationToken cancellationToken = default)
    {
        var appointment = await _appointmentRepository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Appointment), id);

        if (!ActiveStatuses.Contains(appointment.Status))
            throw new ValidationException("Only scheduled or confirmed appointments can be rescheduled.");

        ValidateFutureDateTime(request.AppointmentDate, request.StartTime);
        ValidateTimeRange(request.StartTime, request.EndTime);

        if (await _appointmentRepository.HasDoctorConflictAsync(appointment.DoctorId, request.AppointmentDate.Date, request.StartTime, request.EndTime, id, cancellationToken))
            throw new ConflictException("The selected doctor is already booked for this time.");

        if (await _appointmentRepository.HasPatientConflictAsync(appointment.PatientId, request.AppointmentDate.Date, request.StartTime, request.EndTime, id, cancellationToken))
            throw new ConflictException("The patient already has an overlapping appointment.");

        appointment.AppointmentDate = request.AppointmentDate.Date;
        appointment.StartTime = request.StartTime;
        appointment.EndTime = request.EndTime;
        if (request.Notes is not null)
            appointment.Notes = request.Notes.Trim();
        appointment.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<AppointmentResponse>(appointment);
    }

    public async Task<AppointmentResponse> ConfirmAsync(int id, int? performedByUserId = null, CancellationToken cancellationToken = default)
    {
        var appointment = await GetAppointmentForStatusChangeAsync(id, cancellationToken);
        if (appointment.Status != AppointmentStatus.Scheduled)
            throw new ValidationException("Only scheduled appointments can be confirmed.");

        appointment.Status = AppointmentStatus.Confirmed;
        appointment.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<AppointmentResponse>(appointment);
    }

    public async Task<AppointmentResponse> CancelAsync(int id, string cancellationReason, int? performedByUserId = null, CancellationToken cancellationToken = default)
    {
        var appointment = await GetAppointmentForStatusChangeAsync(id, cancellationToken);
        if (appointment.Status is AppointmentStatus.Cancelled or AppointmentStatus.Completed)
            throw new ValidationException("This appointment cannot be cancelled.");

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.CancellationReason = cancellationReason.Trim();
        appointment.CancelledAt = DateTime.UtcNow;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync(performedByUserId, AuditActions.AppointmentCancelled, nameof(Appointment), appointment.Id, "Appointment cancelled.", cancellationToken: cancellationToken);

        return _mapper.Map<AppointmentResponse>(appointment);
    }

    public async Task<AppointmentResponse> CompleteAsync(int id, int? performedByUserId = null, CancellationToken cancellationToken = default)
    {
        var appointment = await GetAppointmentForStatusChangeAsync(id, cancellationToken);

        if (appointment.Status == AppointmentStatus.Cancelled)
            throw new ValidationException("Cancelled appointments cannot be marked completed.");

        if (appointment.Status == AppointmentStatus.Completed)
            throw new ValidationException("Appointment is already completed.");

        if (!ActiveStatuses.Contains(appointment.Status))
            throw new ValidationException("Only scheduled or confirmed appointments can be completed.");

        appointment.Status = AppointmentStatus.Completed;
        appointment.CompletedAt = DateTime.UtcNow;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync(performedByUserId, AuditActions.AppointmentCompleted, nameof(Appointment), appointment.Id, "Appointment completed.", cancellationToken: cancellationToken);

        return _mapper.Map<AppointmentResponse>(appointment);
    }

    private async Task<Appointment> GetAppointmentForStatusChangeAsync(int id, CancellationToken cancellationToken)
    {
        return await _appointmentRepository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Appointment), id);
    }

    private async Task ValidateEntitiesExistAsync(int patientId, int doctorId, CancellationToken cancellationToken)
    {
        if (await _patientRepository.GetByIdAsync(patientId, cancellationToken) is null)
            throw new NotFoundException(nameof(Patient), patientId);

        if (await _doctorRepository.GetByIdAsync(doctorId, cancellationToken) is null)
            throw new NotFoundException(nameof(Doctor), doctorId);
    }

    private static void ValidateFutureDateTime(DateTime appointmentDate, TimeSpan startTime)
    {
        var appointmentDateTime = appointmentDate.Date.Add(startTime);
        if (appointmentDateTime <= DateTime.UtcNow)
            throw new ValidationException("Appointment date must be in the future.");
    }

    private static void ValidateTimeRange(TimeSpan startTime, TimeSpan endTime)
    {
        if (startTime >= endTime)
            throw new ValidationException("Start time must be before end time.");
    }

    private static void ApplyStatusTransition(Appointment appointment, AppointmentStatus newStatus, string? cancellationReason)
    {
        if (appointment.Status == AppointmentStatus.Cancelled && newStatus == AppointmentStatus.Completed)
            throw new ValidationException("Cancelled appointments cannot be marked completed.");

        if (appointment.Status == AppointmentStatus.Completed && newStatus != AppointmentStatus.Completed)
            throw new ValidationException("Completed appointments cannot be changed.");

        appointment.Status = newStatus;

        switch (newStatus)
        {
            case AppointmentStatus.Cancelled:
                appointment.CancellationReason = cancellationReason?.Trim();
                appointment.CancelledAt = DateTime.UtcNow;
                break;
            case AppointmentStatus.Completed:
                appointment.CompletedAt = DateTime.UtcNow;
                break;
        }
    }
}
