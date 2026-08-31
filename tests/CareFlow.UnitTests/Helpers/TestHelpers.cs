using AutoMapper;
using CareFlow.Application.Mappings;
using CareFlow.Domain.Entities;
using CareFlow.Domain.Enums;
using CareFlow.Shared.Constants;
using CareFlow.Shared.Models;

namespace CareFlow.UnitTests.Helpers;

public static class TestHelpers
{
    public static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        return config.CreateMapper();
    }

    public static Patient CreatePatient(int id = 1, string patientNumber = "PAT-00001")
    {
        var user = new User
        {
            Id = id,
            Email = "patient@test.com",
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "+1-555-0001"
        };

        return new Patient
        {
            Id = id,
            UserId = user.Id,
            User = user,
            PatientNumber = patientNumber,
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = Gender.Male
        };
    }

    public static Doctor CreateDoctor(int id = 1, int departmentId = 1)
    {
        var user = new User
        {
            Id = id + 100,
            Email = "doctor@test.com",
            FirstName = "Jane",
            LastName = "Smith"
        };

        var department = new Department
        {
            Id = departmentId,
            Name = "Cardiology"
        };

        return new Doctor
        {
            Id = id,
            UserId = user.Id,
            User = user,
            DepartmentId = departmentId,
            Department = department,
            LicenseNumber = "MD-12345",
            Specialization = "Cardiology",
            ConsultationFee = 200m
        };
    }

    public static Appointment CreateAppointment(
        int id = 1,
        int patientId = 1,
        int doctorId = 1,
        AppointmentStatus status = AppointmentStatus.Scheduled,
        DateTime? date = null)
    {
        var patient = CreatePatient(patientId);
        var doctor = CreateDoctor(doctorId);
        var appointmentDate = date ?? DateTime.UtcNow.Date.AddDays(2);

        return new Appointment
        {
            Id = id,
            PatientId = patientId,
            Patient = patient,
            DoctorId = doctorId,
            Doctor = doctor,
            AppointmentDate = appointmentDate,
            StartTime = new TimeSpan(10, 0, 0),
            EndTime = new TimeSpan(10, 30, 0),
            Status = status,
            Reason = "Checkup"
        };
    }

    public static PagedResult<T> CreatePagedResult<T>(IEnumerable<T> items, int pageNumber = 1, int pageSize = 10)
    {
        var list = items.ToList();
        return new PagedResult<T>
        {
            Items = list,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = list.Count
        };
    }

    public static Role CreateRole(string name = Roles.Patient) =>
        new() { Id = 1, Name = name, Description = $"{name} role" };
}
