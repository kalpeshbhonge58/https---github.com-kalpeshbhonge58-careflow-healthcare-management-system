using CareFlow.Application.DTOs.Appointment;
using CareFlow.Application.Exceptions;
using CareFlow.Application.Interfaces;
using CareFlow.Application.Services;
using CareFlow.Application.Validators;
using CareFlow.Domain.Entities;
using CareFlow.Domain.Enums;
using CareFlow.Shared.Models;
using CareFlow.UnitTests.Helpers;
using FluentValidation;
using Moq;

namespace CareFlow.UnitTests.Services;

public class AppointmentServiceTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepo = new();
    private readonly Mock<IRepository<Appointment>> _appointmentEntityRepo = new();
    private readonly Mock<IRepository<Patient>> _patientRepo = new();
    private readonly Mock<IRepository<Doctor>> _doctorRepo = new();
    private readonly Mock<IAuditService> _auditService = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly IValidator<CreateAppointmentRequest> _validator = new CreateAppointmentRequestValidator();
    private readonly AppointmentService _sut;

    public AppointmentServiceTests()
    {
        _sut = new AppointmentService(
            _appointmentRepo.Object,
            _appointmentEntityRepo.Object,
            _patientRepo.Object,
            _doctorRepo.Object,
            _auditService.Object,
            _unitOfWork.Object,
            TestHelpers.CreateMapper(),
            _validator);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsAppointment()
    {
        var futureDate = DateTime.UtcNow.Date.AddDays(3);
        var request = new CreateAppointmentRequest
        {
            PatientId = 1,
            DoctorId = 1,
            AppointmentDate = futureDate,
            StartTime = new TimeSpan(14, 0, 0),
            EndTime = new TimeSpan(14, 30, 0),
            Reason = "Annual checkup"
        };

        var appointment = TestHelpers.CreateAppointment(10, 1, 1, AppointmentStatus.Scheduled, futureDate);
        appointment.StartTime = request.StartTime;
        appointment.EndTime = request.EndTime;

        _patientRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(appointment.Patient);
        _doctorRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(appointment.Doctor);
        _appointmentRepo.Setup(r => r.HasDoctorConflictAsync(1, futureDate, request.StartTime, request.EndTime, null, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _appointmentRepo.Setup(r => r.HasPatientConflictAsync(1, futureDate, request.StartTime, request.EndTime, null, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _appointmentEntityRepo.Setup(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()))
            .Callback<Appointment, CancellationToken>((a, _) => a.Id = 10)
            .ReturnsAsync((Appointment a, CancellationToken _) => a);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _appointmentRepo.Setup(r => r.GetByIdWithDetailsAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(appointment);

        var result = await _sut.CreateAsync(request);

        Assert.Equal(10, result.Id);
        Assert.Equal("John Doe", result.PatientName);
        Assert.Equal("Scheduled", result.Status);
        _auditService.Verify(a => a.LogAsync(null, It.IsAny<string>(), nameof(Appointment), 10, It.IsAny<string>(), null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_DuplicateDoctorBooking_ThrowsConflictException()
    {
        var futureDate = DateTime.UtcNow.Date.AddDays(3);
        var request = new CreateAppointmentRequest
        {
            PatientId = 1,
            DoctorId = 1,
            AppointmentDate = futureDate,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(9, 30, 0)
        };

        _patientRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(TestHelpers.CreatePatient());
        _doctorRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(TestHelpers.CreateDoctor());
        _appointmentRepo.Setup(r => r.HasDoctorConflictAsync(1, futureDate, request.StartTime, request.EndTime, null, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var ex = await Assert.ThrowsAsync<ConflictException>(() => _sut.CreateAsync(request));
        Assert.Contains("doctor is already booked", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_PatientOverlap_ThrowsConflictException()
    {
        var futureDate = DateTime.UtcNow.Date.AddDays(3);
        var request = new CreateAppointmentRequest
        {
            PatientId = 1,
            DoctorId = 1,
            AppointmentDate = futureDate,
            StartTime = new TimeSpan(11, 0, 0),
            EndTime = new TimeSpan(11, 30, 0)
        };

        _patientRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(TestHelpers.CreatePatient());
        _doctorRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(TestHelpers.CreateDoctor());
        _appointmentRepo.Setup(r => r.HasDoctorConflictAsync(1, futureDate, request.StartTime, request.EndTime, null, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _appointmentRepo.Setup(r => r.HasPatientConflictAsync(1, futureDate, request.StartTime, request.EndTime, null, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var ex = await Assert.ThrowsAsync<ConflictException>(() => _sut.CreateAsync(request));
        Assert.Contains("overlapping appointment", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CompleteAsync_CancelledAppointment_ThrowsValidationException()
    {
        var appointment = TestHelpers.CreateAppointment(status: AppointmentStatus.Cancelled);
        _appointmentRepo.Setup(r => r.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(appointment);

        var ex = await Assert.ThrowsAsync<CareFlow.Application.Exceptions.ValidationException>(() => _sut.CompleteAsync(1));
        Assert.Contains("Cancelled appointments cannot be marked completed", ex.Message);
    }

    [Fact]
    public async Task CreateAsync_PastDate_ThrowsValidationException()
    {
        var request = new CreateAppointmentRequest
        {
            PatientId = 1,
            DoctorId = 1,
            AppointmentDate = DateTime.UtcNow.Date.AddDays(-1),
            StartTime = new TimeSpan(10, 0, 0),
            EndTime = new TimeSpan(10, 30, 0)
        };

        await Assert.ThrowsAsync<CareFlow.Application.Exceptions.ValidationException>(() => _sut.CreateAsync(request));
    }
}
