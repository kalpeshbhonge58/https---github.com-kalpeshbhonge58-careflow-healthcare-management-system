using AutoMapper;
using CareFlow.Application.DTOs.Dashboard;
using CareFlow.Application.Interfaces;
using CareFlow.Shared.Constants;

namespace CareFlow.Application.Services;

public class DashboardService : IDashboardService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private readonly IDashboardRepository _dashboardRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IPrescriptionRepository _prescriptionRepository;
    private readonly IMedicalRecordRepository _medicalRecordRepository;
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;

    public DashboardService(
        IDashboardRepository dashboardRepository,
        IAppointmentRepository appointmentRepository,
        IPrescriptionRepository prescriptionRepository,
        IMedicalRecordRepository medicalRecordRepository,
        ICacheService cacheService,
        IMapper mapper)
    {
        _dashboardRepository = dashboardRepository;
        _appointmentRepository = appointmentRepository;
        _prescriptionRepository = prescriptionRepository;
        _medicalRecordRepository = medicalRecordRepository;
        _cacheService = cacheService;
        _mapper = mapper;
    }

    public async Task<AdminDashboardResponse> GetAdminDashboardAsync(CancellationToken cancellationToken = default)
    {
        var cached = await _cacheService.GetAsync<AdminDashboardResponse>(CacheKeys.AdminDashboard, cancellationToken);
        if (cached is not null)
            return cached;

        var now = DateTime.UtcNow;
        var deptStats = await _dashboardRepository.GetAppointmentsByDepartmentAsync(cancellationToken);
        var doctorStats = await _dashboardRepository.GetAppointmentsByDoctorAsync(cancellationToken);

        var response = new AdminDashboardResponse
        {
            TotalPatients = await _dashboardRepository.GetTotalPatientsAsync(cancellationToken),
            TotalDoctors = await _dashboardRepository.GetTotalDoctorsAsync(cancellationToken),
            TodayAppointments = await _dashboardRepository.GetTodayAppointmentsCountAsync(cancellationToken),
            PendingConfirmations = await _dashboardRepository.GetPendingConfirmationsCountAsync(cancellationToken),
            MonthlyRevenue = await _dashboardRepository.GetMonthlyRevenueAsync(now.Year, now.Month, cancellationToken),
            PendingPayments = await _dashboardRepository.GetPendingPaymentsTotalAsync(cancellationToken),
            AppointmentsByDepartment = deptStats.Select(d => new DepartmentAppointmentStat
            {
                Department = d.Department,
                Count = d.Count
            }).ToList(),
            AppointmentsByDoctor = doctorStats.Select(d => new DoctorAppointmentStat
            {
                DoctorId = d.DoctorId,
                DoctorName = d.DoctorName,
                Count = d.Count
            }).ToList()
        };

        await _cacheService.SetAsync(CacheKeys.AdminDashboard, response, CacheDuration, cancellationToken);
        return response;
    }

    public async Task<DoctorDashboardResponse> GetDoctorDashboardAsync(int doctorId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CacheKeys.DoctorDashboardPrefix}{doctorId}";
        var cached = await _cacheService.GetAsync<DoctorDashboardResponse>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var upcoming = await _appointmentRepository.GetUpcomingByDoctorAsync(doctorId, 10, cancellationToken);

        var response = new DoctorDashboardResponse
        {
            TodayAppointments = await _dashboardRepository.GetDoctorTodayAppointmentsCountAsync(doctorId, cancellationToken),
            CompletedAppointments = await _dashboardRepository.GetDoctorCompletedAppointmentsCountAsync(doctorId, cancellationToken),
            PatientCount = await _dashboardRepository.GetDoctorPatientCountAsync(doctorId, cancellationToken),
            UpcomingAppointments = _mapper.Map<IReadOnlyList<DTOs.Appointment.AppointmentResponse>>(upcoming)
        };

        await _cacheService.SetAsync(cacheKey, response, CacheDuration, cancellationToken);
        return response;
    }

    public async Task<ReceptionistDashboardResponse> GetReceptionistDashboardAsync(CancellationToken cancellationToken = default)
    {
        var cached = await _cacheService.GetAsync<ReceptionistDashboardResponse>(CacheKeys.ReceptionistDashboard, cancellationToken);
        if (cached is not null)
            return cached;

        var response = new ReceptionistDashboardResponse
        {
            TodayAppointments = await _dashboardRepository.GetTodayAppointmentsCountAsync(cancellationToken),
            PendingConfirmations = await _dashboardRepository.GetPendingConfirmationsCountAsync(cancellationToken),
            TotalPatients = await _dashboardRepository.GetTotalPatientsAsync(cancellationToken),
            TotalDoctors = await _dashboardRepository.GetTotalDoctorsAsync(cancellationToken)
        };

        await _cacheService.SetAsync(CacheKeys.ReceptionistDashboard, response, CacheDuration, cancellationToken);
        return response;
    }

    public async Task<PatientDashboardResponse> GetPatientDashboardAsync(int patientId, CancellationToken cancellationToken = default)
    {
        var upcoming = await _appointmentRepository.GetUpcomingByPatientAsync(patientId, 5, cancellationToken);

        var appointmentPaged = await _appointmentRepository.GetPagedAsync(
            new Shared.Models.PaginationQuery { PageNumber = 1, PageSize = 5, SortBy = "AppointmentDate", SortDirection = "desc" },
            patientId,
            cancellationToken: cancellationToken);

        var prescriptionPaged = await _prescriptionRepository.GetPagedAsync(
            new Shared.Models.PaginationQuery { PageNumber = 1, PageSize = 5, SortBy = "PrescriptionDate", SortDirection = "desc" },
            patientId,
            cancellationToken: cancellationToken);

        var medicalRecordPaged = await _medicalRecordRepository.GetPagedAsync(
            new Shared.Models.PaginationQuery { PageNumber = 1, PageSize = 5, SortBy = "VisitDate", SortDirection = "desc" },
            patientId,
            cancellationToken: cancellationToken);

        return new PatientDashboardResponse
        {
            UpcomingAppointments = _mapper.Map<IReadOnlyList<DTOs.Appointment.AppointmentResponse>>(upcoming),
            RecentAppointments = _mapper.Map<IReadOnlyList<DTOs.Appointment.AppointmentResponse>>(appointmentPaged.Items),
            RecentPrescriptions = _mapper.Map<IReadOnlyList<DTOs.Prescription.PrescriptionResponse>>(prescriptionPaged.Items),
            RecentMedicalRecords = _mapper.Map<IReadOnlyList<DTOs.MedicalRecord.MedicalRecordResponse>>(medicalRecordPaged.Items)
        };
    }
}
