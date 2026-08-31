using CareFlow.Application.DTOs.Patient;
using CareFlow.Application.Exceptions;
using CareFlow.Application.Interfaces;
using CareFlow.Application.Services;
using CareFlow.Application.Validators;
using CareFlow.Domain.Entities;
using CareFlow.Domain.Enums;
using CareFlow.Shared.Constants;
using CareFlow.Shared.Models;
using CareFlow.UnitTests.Helpers;
using FluentValidation;
using Moq;

namespace CareFlow.UnitTests.Services;

public class PatientServiceTests
{
    private readonly Mock<IPatientRepository> _patientRepo = new();
    private readonly Mock<IRepository<User>> _userRepo = new();
    private readonly Mock<IRepository<Patient>> _patientEntityRepo = new();
    private readonly Mock<IRepository<Role>> _roleRepo = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IAuditService> _auditService = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly IValidator<CreatePatientRequest> _validator = new CreatePatientRequestValidator();
    private readonly PatientService _sut;

    public PatientServiceTests()
    {
        _sut = new PatientService(
            _patientRepo.Object,
            _userRepo.Object,
            _patientEntityRepo.Object,
            _roleRepo.Object,
            _passwordHasher.Object,
            _auditService.Object,
            _unitOfWork.Object,
            TestHelpers.CreateMapper(),
            _validator);

        _passwordHasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed-password");
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsMappedResults()
    {
        var patients = new[] { TestHelpers.CreatePatient() };
        _patientRepo.Setup(r => r.GetPagedAsync(It.IsAny<PaginationQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestHelpers.CreatePagedResult(patients));

        var result = await _sut.GetPagedAsync(new PaginationQuery());

        Assert.Single(result.Items);
        Assert.Equal("John Doe", result.Items.First().FullName);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingPatient_ReturnsDetail()
    {
        var patient = TestHelpers.CreatePatient();
        patient.Appointments.Add(TestHelpers.CreateAppointment(patientId: patient.Id));
        patient.MedicalRecords.Add(new MedicalRecord { Id = 1, PatientId = patient.Id, DoctorId = 1, VisitDate = DateTime.UtcNow, Diagnosis = "Test" });

        _patientRepo.Setup(r => r.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(patient);

        var result = await _sut.GetByIdAsync(1);

        Assert.Equal(1, result.TotalAppointments);
        Assert.Equal(1, result.TotalMedicalRecords);
        Assert.Equal("John Doe", result.FullName);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ThrowsNotFoundException()
    {
        _patientRepo.Setup(r => r.GetByIdWithDetailsAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Patient?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(99));
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesPatient()
    {
        var request = new CreatePatientRequest
        {
            Email = "new.patient@test.com",
            Password = "Password123",
            FirstName = "Alice",
            LastName = "Johnson",
            DateOfBirth = new DateTime(1995, 5, 5),
            Gender = Gender.Female
        };

        var createdPatient = TestHelpers.CreatePatient(5);
        createdPatient.User.Email = request.Email;
        createdPatient.User.FirstName = request.FirstName;
        createdPatient.User.LastName = request.LastName;

        _userRepo.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _roleRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Role, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Role> { TestHelpers.CreateRole() });
        _userRepo.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).ReturnsAsync((User u, CancellationToken _) => u);
        _patientEntityRepo.Setup(r => r.AddAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()))
            .Callback<Patient, CancellationToken>((p, _) => p.Id = 5)
            .ReturnsAsync((Patient p, CancellationToken _) => p);
        _patientRepo.Setup(r => r.PatientNumberExistsAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _patientRepo.Setup(r => r.GetByIdWithDetailsAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(createdPatient);

        var result = await _sut.CreateAsync(request);

        Assert.Equal(5, result.Id);
        Assert.Equal("Alice", result.FirstName);
        _auditService.Verify(a => a.LogAsync(null, AuditActions.PatientCreated, nameof(Patient), 5, It.IsAny<string>(), null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_UpdatesPatient()
    {
        var patient = TestHelpers.CreatePatient();
        var request = new UpdatePatientRequest
        {
            FirstName = "Updated",
            LastName = "Name",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = Gender.Male
        };

        _patientRepo.Setup(r => r.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(patient);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.UpdateAsync(1, request);

        Assert.Equal("Updated", result.FirstName);
        Assert.Equal("Name", result.LastName);
    }

    [Fact]
    public async Task DeactivateAsync_ExistingPatient_DeactivatesPatientAndUser()
    {
        var patient = TestHelpers.CreatePatient();
        _patientRepo.Setup(r => r.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(patient);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        await _sut.DeactivateAsync(1);

        Assert.False(patient.IsActive);
        Assert.False(patient.User.IsActive);
        _auditService.Verify(a => a.LogAsync(null, AuditActions.PatientDeactivated, nameof(Patient), 1, It.IsAny<string>(), null, It.IsAny<CancellationToken>()), Times.Once);
    }
}
