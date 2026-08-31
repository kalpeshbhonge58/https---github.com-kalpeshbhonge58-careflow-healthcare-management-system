using CareFlow.Application.DTOs.Doctor;
using CareFlow.Application.Exceptions;
using CareFlow.Application.Interfaces;
using CareFlow.Application.Services;
using CareFlow.Domain.Entities;
using CareFlow.Shared.Constants;
using CareFlow.Shared.Models;
using CareFlow.UnitTests.Helpers;
using Moq;

namespace CareFlow.UnitTests.Services;

public class DoctorServiceTests
{
    private readonly Mock<IDoctorRepository> _doctorRepo = new();
    private readonly Mock<IRepository<User>> _userRepo = new();
    private readonly Mock<IRepository<Doctor>> _doctorEntityRepo = new();
    private readonly Mock<IRepository<Role>> _roleRepo = new();
    private readonly Mock<IRepository<Department>> _departmentRepo = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IAuditService> _auditService = new();
    private readonly Mock<ICacheService> _cacheService = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly DoctorService _sut;

    public DoctorServiceTests()
    {
        _sut = new DoctorService(
            _doctorRepo.Object,
            _userRepo.Object,
            _doctorEntityRepo.Object,
            _roleRepo.Object,
            _departmentRepo.Object,
            _passwordHasher.Object,
            _auditService.Object,
            _cacheService.Object,
            _unitOfWork.Object,
            TestHelpers.CreateMapper());

        _passwordHasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed-password");
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsMappedDoctors()
    {
        var doctors = new[] { TestHelpers.CreateDoctor() };
        _doctorRepo.Setup(r => r.GetPagedAsync(It.IsAny<PaginationQuery>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestHelpers.CreatePagedResult(doctors));

        var result = await _sut.GetPagedAsync(new PaginationQuery());

        Assert.Single(result.Items);
        Assert.Equal("Jane Smith", result.Items.First().FullName);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingDoctor_ReturnsDoctor()
    {
        var doctor = TestHelpers.CreateDoctor();
        _doctorRepo.Setup(r => r.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(doctor);

        var result = await _sut.GetByIdAsync(1);

        Assert.Equal("Cardiology", result.DepartmentName);
    }

    [Fact]
    public async Task GetActiveDoctorsAsync_CacheHit_ReturnsCachedData()
    {
        var cached = new List<DoctorResponse> { new() { Id = 1, FullName = "Cached Doctor" } }.AsReadOnly();
        _cacheService.Setup(c => c.GetAsync<IReadOnlyList<DoctorResponse>>(CacheKeys.DoctorsLookup, It.IsAny<CancellationToken>())).ReturnsAsync(cached);

        var result = await _sut.GetActiveDoctorsAsync();

        Assert.Single(result);
        Assert.Equal("Cached Doctor", result[0].FullName);
        _doctorRepo.Verify(r => r.GetActiveDoctorsAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetActiveDoctorsAsync_CacheMiss_FetchesAndCaches()
    {
        var doctors = new List<Doctor> { TestHelpers.CreateDoctor() };
        _cacheService.Setup(c => c.GetAsync<IReadOnlyList<DoctorResponse>>(CacheKeys.DoctorsLookup, It.IsAny<CancellationToken>())).ReturnsAsync((IReadOnlyList<DoctorResponse>?)null);
        _doctorRepo.Setup(r => r.GetActiveDoctorsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(doctors);

        var result = await _sut.GetActiveDoctorsAsync();

        Assert.Single(result);
        _cacheService.Verify(c => c.SetAsync(CacheKeys.DoctorsLookup, It.IsAny<IReadOnlyList<DoctorResponse>>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesDoctor()
    {
        var request = new CreateDoctorRequest
        {
            Email = "new.doctor@test.com",
            Password = "Password123",
            FirstName = "New",
            LastName = "Doctor",
            DepartmentId = 1,
            LicenseNumber = "MD-99999",
            Specialization = "Pediatrics",
            ConsultationFee = 150m
        };

        var created = TestHelpers.CreateDoctor(3);
        _userRepo.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _departmentRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new Department { Id = 1, Name = "Pediatrics" });
        _roleRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Role, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Role> { TestHelpers.CreateRole(Roles.Doctor) });
        _userRepo.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).ReturnsAsync((User u, CancellationToken _) => u);
        _doctorEntityRepo.Setup(r => r.AddAsync(It.IsAny<Doctor>(), It.IsAny<CancellationToken>()))
            .Callback<Doctor, CancellationToken>((d, _) => d.Id = 3)
            .ReturnsAsync((Doctor d, CancellationToken _) => d);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _doctorRepo.Setup(r => r.GetByIdWithDetailsAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(created);

        var result = await _sut.CreateAsync(request);

        Assert.Equal(3, result.Id);
        _cacheService.Verify(c => c.RemoveAsync(CacheKeys.DoctorsLookup, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_UpdatesDoctor()
    {
        var doctor = TestHelpers.CreateDoctor();
        var request = new UpdateDoctorRequest
        {
            FirstName = "Updated",
            LastName = "Doctor",
            DepartmentId = 1,
            LicenseNumber = "MD-UPDATED",
            Specialization = "Neurology",
            ConsultationFee = 350m
        };

        _doctorRepo.Setup(r => r.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(doctor);
        _departmentRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(doctor.Department);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.UpdateAsync(1, request);

        Assert.Equal("Updated", result.FirstName);
        _cacheService.Verify(c => c.RemoveAsync(CacheKeys.DoctorsLookup, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeactivateAsync_ExistingDoctor_DeactivatesDoctor()
    {
        var doctor = TestHelpers.CreateDoctor();
        _doctorRepo.Setup(r => r.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(doctor);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        await _sut.DeactivateAsync(1);

        Assert.False(doctor.IsActive);
        Assert.False(doctor.User.IsActive);
    }
}
