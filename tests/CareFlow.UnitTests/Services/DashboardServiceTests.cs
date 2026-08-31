using CareFlow.Application.DTOs.Dashboard;
using CareFlow.Application.Interfaces;
using CareFlow.Application.Services;
using CareFlow.Shared.Constants;
using Moq;

namespace CareFlow.UnitTests.Services;

public class DashboardServiceTests
{
    private readonly Mock<IDashboardRepository> _dashboardRepo = new();
    private readonly Mock<IAppointmentRepository> _appointmentRepo = new();
    private readonly Mock<IPrescriptionRepository> _prescriptionRepo = new();
    private readonly Mock<IMedicalRecordRepository> _medicalRecordRepo = new();
    private readonly Mock<ICacheService> _cacheService = new();
    private readonly DashboardService _sut;

    public DashboardServiceTests()
    {
        _sut = new DashboardService(
            _dashboardRepo.Object,
            _appointmentRepo.Object,
            _prescriptionRepo.Object,
            _medicalRecordRepo.Object,
            _cacheService.Object,
            CareFlow.UnitTests.Helpers.TestHelpers.CreateMapper());
    }

    [Fact]
    public async Task GetAdminDashboardAsync_CacheHit_ReturnsCachedResponse()
    {
        var cached = new AdminDashboardResponse { TotalPatients = 42, TotalDoctors = 7 };
        _cacheService.Setup(c => c.GetAsync<AdminDashboardResponse>(CacheKeys.AdminDashboard, It.IsAny<CancellationToken>())).ReturnsAsync(cached);

        var result = await _sut.GetAdminDashboardAsync();

        Assert.Equal(42, result.TotalPatients);
        Assert.Equal(7, result.TotalDoctors);
        _dashboardRepo.Verify(r => r.GetTotalPatientsAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAdminDashboardAsync_CacheMiss_FetchesDataAndCaches()
    {
        _cacheService.Setup(c => c.GetAsync<AdminDashboardResponse>(CacheKeys.AdminDashboard, It.IsAny<CancellationToken>())).ReturnsAsync((AdminDashboardResponse?)null);
        _dashboardRepo.Setup(r => r.GetTotalPatientsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(10);
        _dashboardRepo.Setup(r => r.GetTotalDoctorsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(3);
        _dashboardRepo.Setup(r => r.GetTodayAppointmentsCountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(5);
        _dashboardRepo.Setup(r => r.GetPendingConfirmationsCountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(2);
        _dashboardRepo.Setup(r => r.GetMonthlyRevenueAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(1500m);
        _dashboardRepo.Setup(r => r.GetPendingPaymentsTotalAsync(It.IsAny<CancellationToken>())).ReturnsAsync(300m);
        _dashboardRepo.Setup(r => r.GetAppointmentsByDepartmentAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<(string, int)> { ("Cardiology", 4) });
        _dashboardRepo.Setup(r => r.GetAppointmentsByDoctorAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<(int, string, int)> { (1, "Dr. Smith", 3) });

        var result = await _sut.GetAdminDashboardAsync();

        Assert.Equal(10, result.TotalPatients);
        Assert.Equal(3, result.TotalDoctors);
        Assert.Single(result.AppointmentsByDepartment);
        _cacheService.Verify(c => c.SetAsync(CacheKeys.AdminDashboard, It.IsAny<AdminDashboardResponse>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetReceptionistDashboardAsync_CacheHit_ReturnsCachedResponse()
    {
        var cached = new ReceptionistDashboardResponse { TodayAppointments = 8 };
        _cacheService.Setup(c => c.GetAsync<ReceptionistDashboardResponse>(CacheKeys.ReceptionistDashboard, It.IsAny<CancellationToken>())).ReturnsAsync(cached);

        var result = await _sut.GetReceptionistDashboardAsync();

        Assert.Equal(8, result.TodayAppointments);
        _dashboardRepo.Verify(r => r.GetTodayAppointmentsCountAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetDoctorDashboardAsync_CacheMiss_UsesDoctorSpecificCacheKey()
    {
        var doctorId = 5;
        var cacheKey = $"{CacheKeys.DoctorDashboardPrefix}{doctorId}";
        _cacheService.Setup(c => c.GetAsync<DoctorDashboardResponse>(cacheKey, It.IsAny<CancellationToken>())).ReturnsAsync((DoctorDashboardResponse?)null);
        _dashboardRepo.Setup(r => r.GetDoctorTodayAppointmentsCountAsync(doctorId, It.IsAny<CancellationToken>())).ReturnsAsync(2);
        _dashboardRepo.Setup(r => r.GetDoctorCompletedAppointmentsCountAsync(doctorId, It.IsAny<CancellationToken>())).ReturnsAsync(10);
        _dashboardRepo.Setup(r => r.GetDoctorPatientCountAsync(doctorId, It.IsAny<CancellationToken>())).ReturnsAsync(15);
        _appointmentRepo.Setup(r => r.GetUpcomingByDoctorAsync(doctorId, 10, It.IsAny<CancellationToken>())).ReturnsAsync(new List<CareFlow.Domain.Entities.Appointment>());

        var result = await _sut.GetDoctorDashboardAsync(doctorId);

        Assert.Equal(2, result.TodayAppointments);
        _cacheService.Verify(c => c.SetAsync(cacheKey, It.IsAny<DoctorDashboardResponse>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
