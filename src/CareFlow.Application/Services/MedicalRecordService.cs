using AutoMapper;
using CareFlow.Application.DTOs.MedicalRecord;
using CareFlow.Application.Exceptions;
using CareFlow.Application.Interfaces;
using CareFlow.Domain.Entities;
using CareFlow.Shared.Models;

namespace CareFlow.Application.Services;

public class MedicalRecordService : IMedicalRecordService
{
    private readonly IMedicalRecordRepository _medicalRecordRepository;
    private readonly IRepository<MedicalRecord> _medicalRecordEntityRepository;
    private readonly IRepository<Patient> _patientRepository;
    private readonly IRepository<Doctor> _doctorRepository;
    private readonly IRepository<Appointment> _appointmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public MedicalRecordService(
        IMedicalRecordRepository medicalRecordRepository,
        IRepository<MedicalRecord> medicalRecordEntityRepository,
        IRepository<Patient> patientRepository,
        IRepository<Doctor> doctorRepository,
        IRepository<Appointment> appointmentRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _medicalRecordRepository = medicalRecordRepository;
        _medicalRecordEntityRepository = medicalRecordEntityRepository;
        _patientRepository = patientRepository;
        _doctorRepository = doctorRepository;
        _appointmentRepository = appointmentRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<MedicalRecordResponse>> GetPagedAsync(PaginationQuery query, int? patientId = null, int? doctorId = null, CancellationToken cancellationToken = default)
    {
        var paged = await _medicalRecordRepository.GetPagedAsync(query, patientId, doctorId, cancellationToken);
        return new PagedResult<MedicalRecordResponse>
        {
            Items = _mapper.Map<IEnumerable<MedicalRecordResponse>>(paged.Items),
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize,
            TotalRecords = paged.TotalRecords
        };
    }

    public async Task<MedicalRecordResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var record = await _medicalRecordRepository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(MedicalRecord), id);

        return _mapper.Map<MedicalRecordResponse>(record);
    }

    public async Task<IReadOnlyList<MedicalRecordResponse>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default)
    {
        var records = await _medicalRecordRepository.GetByPatientIdAsync(patientId, cancellationToken);
        return _mapper.Map<IReadOnlyList<MedicalRecordResponse>>(records);
    }

    public async Task<MedicalRecordResponse> CreateAsync(CreateMedicalRecordRequest request, int? performedByUserId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Diagnosis))
            throw new ValidationException("Diagnosis is required.");

        if (await _patientRepository.GetByIdAsync(request.PatientId, cancellationToken) is null)
            throw new NotFoundException(nameof(Patient), request.PatientId);

        if (await _doctorRepository.GetByIdAsync(request.DoctorId, cancellationToken) is null)
            throw new NotFoundException(nameof(Doctor), request.DoctorId);

        if (request.AppointmentId.HasValue &&
            await _appointmentRepository.GetByIdAsync(request.AppointmentId.Value, cancellationToken) is null)
            throw new NotFoundException(nameof(Appointment), request.AppointmentId.Value);

        var record = new MedicalRecord
        {
            PatientId = request.PatientId,
            DoctorId = request.DoctorId,
            AppointmentId = request.AppointmentId,
            VisitDate = request.VisitDate,
            Diagnosis = request.Diagnosis.Trim(),
            Symptoms = request.Symptoms?.Trim(),
            Treatment = request.Treatment?.Trim(),
            Notes = request.Notes?.Trim(),
            DoctorComments = request.DoctorComments?.Trim(),
            FollowUpDate = request.FollowUpDate
        };

        await _medicalRecordEntityRepository.AddAsync(record, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _medicalRecordRepository.GetByIdWithDetailsAsync(record.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(MedicalRecord), record.Id);

        return _mapper.Map<MedicalRecordResponse>(created);
    }

    public async Task<MedicalRecordResponse> UpdateAsync(int id, UpdateMedicalRecordRequest request, int? performedByUserId = null, CancellationToken cancellationToken = default)
    {
        var record = await _medicalRecordRepository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(MedicalRecord), id);

        if (string.IsNullOrWhiteSpace(request.Diagnosis))
            throw new ValidationException("Diagnosis is required.");

        record.Diagnosis = request.Diagnosis.Trim();
        record.Symptoms = request.Symptoms?.Trim();
        record.Treatment = request.Treatment?.Trim();
        record.Notes = request.Notes?.Trim();
        record.DoctorComments = request.DoctorComments?.Trim();
        record.FollowUpDate = request.FollowUpDate;
        record.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<MedicalRecordResponse>(record);
    }
}
