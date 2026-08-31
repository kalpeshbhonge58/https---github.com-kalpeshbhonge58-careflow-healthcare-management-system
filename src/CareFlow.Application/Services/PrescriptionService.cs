using AutoMapper;
using CareFlow.Application.DTOs.Prescription;
using CareFlow.Application.Exceptions;
using CareFlow.Application.Interfaces;
using CareFlow.Domain.Entities;
using CareFlow.Shared.Models;

namespace CareFlow.Application.Services;

public class PrescriptionService : IPrescriptionService
{
    private readonly IPrescriptionRepository _prescriptionRepository;
    private readonly IRepository<Prescription> _prescriptionEntityRepository;
    private readonly IRepository<PrescriptionItem> _prescriptionItemRepository;
    private readonly IRepository<Patient> _patientRepository;
    private readonly IRepository<Doctor> _doctorRepository;
    private readonly IRepository<Medicine> _medicineRepository;
    private readonly IRepository<MedicalRecord> _medicalRecordRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PrescriptionService(
        IPrescriptionRepository prescriptionRepository,
        IRepository<Prescription> prescriptionEntityRepository,
        IRepository<PrescriptionItem> prescriptionItemRepository,
        IRepository<Patient> patientRepository,
        IRepository<Doctor> doctorRepository,
        IRepository<Medicine> medicineRepository,
        IRepository<MedicalRecord> medicalRecordRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _prescriptionRepository = prescriptionRepository;
        _prescriptionEntityRepository = prescriptionEntityRepository;
        _prescriptionItemRepository = prescriptionItemRepository;
        _patientRepository = patientRepository;
        _doctorRepository = doctorRepository;
        _medicineRepository = medicineRepository;
        _medicalRecordRepository = medicalRecordRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<PrescriptionResponse>> GetPagedAsync(PaginationQuery query, int? patientId = null, int? doctorId = null, CancellationToken cancellationToken = default)
    {
        var paged = await _prescriptionRepository.GetPagedAsync(query, patientId, doctorId, cancellationToken);
        return new PagedResult<PrescriptionResponse>
        {
            Items = _mapper.Map<IEnumerable<PrescriptionResponse>>(paged.Items),
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize,
            TotalRecords = paged.TotalRecords
        };
    }

    public async Task<PrescriptionResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var prescription = await _prescriptionRepository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Prescription), id);

        return _mapper.Map<PrescriptionResponse>(prescription);
    }

    public async Task<PrescriptionResponse> CreateAsync(CreatePrescriptionRequest request, int? performedByUserId = null, CancellationToken cancellationToken = default)
    {
        if (request.Items.Count == 0)
            throw new ValidationException("At least one prescription item is required.");

        if (await _patientRepository.GetByIdAsync(request.PatientId, cancellationToken) is null)
            throw new NotFoundException(nameof(Patient), request.PatientId);

        if (await _doctorRepository.GetByIdAsync(request.DoctorId, cancellationToken) is null)
            throw new NotFoundException(nameof(Doctor), request.DoctorId);

        if (request.MedicalRecordId.HasValue &&
            await _medicalRecordRepository.GetByIdAsync(request.MedicalRecordId.Value, cancellationToken) is null)
            throw new NotFoundException(nameof(MedicalRecord), request.MedicalRecordId.Value);

        foreach (var item in request.Items)
        {
            if (await _medicineRepository.GetByIdAsync(item.MedicineId, cancellationToken) is null)
                throw new NotFoundException(nameof(Medicine), item.MedicineId);
        }

        var prescription = new Prescription
        {
            PatientId = request.PatientId,
            DoctorId = request.DoctorId,
            MedicalRecordId = request.MedicalRecordId,
            PrescriptionDate = request.PrescriptionDate,
            Instructions = request.Instructions?.Trim(),
            Notes = request.Notes?.Trim()
        };

        await _prescriptionEntityRepository.AddAsync(prescription, cancellationToken);

        foreach (var itemRequest in request.Items)
        {
            var item = new PrescriptionItem
            {
                Prescription = prescription,
                MedicineId = itemRequest.MedicineId,
                Dosage = itemRequest.Dosage.Trim(),
                Frequency = itemRequest.Frequency.Trim(),
                DurationDays = itemRequest.DurationDays,
                Instructions = itemRequest.Instructions?.Trim(),
                Quantity = itemRequest.Quantity
            };
            await _prescriptionItemRepository.AddAsync(item, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _prescriptionRepository.GetByIdWithDetailsAsync(prescription.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Prescription), prescription.Id);

        return _mapper.Map<PrescriptionResponse>(created);
    }
}
