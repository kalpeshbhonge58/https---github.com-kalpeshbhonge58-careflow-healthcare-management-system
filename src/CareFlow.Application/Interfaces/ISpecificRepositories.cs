using CareFlow.Domain.Entities;
using CareFlow.Shared.Models;

namespace CareFlow.Application.Interfaces;

public interface IPatientRepository
{
    Task<Patient?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<Patient?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<PagedResult<Patient>> GetPagedAsync(PaginationQuery query, CancellationToken cancellationToken = default);
    Task<bool> PatientNumberExistsAsync(string patientNumber, int? excludeId = null, CancellationToken cancellationToken = default);
}

public interface IDoctorRepository
{
    Task<Doctor?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<Doctor?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<PagedResult<Doctor>> GetPagedAsync(PaginationQuery query, int? departmentId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Doctor>> GetActiveDoctorsAsync(CancellationToken cancellationToken = default);
}

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResult<Appointment>> GetPagedAsync(PaginationQuery query, int? patientId = null, int? doctorId = null, string? status = null, CancellationToken cancellationToken = default);
    Task<bool> HasDoctorConflictAsync(int doctorId, DateTime date, TimeSpan start, TimeSpan end, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> HasPatientConflictAsync(int patientId, DateTime date, TimeSpan start, TimeSpan end, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Appointment>> GetUpcomingByDoctorAsync(int doctorId, int count, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Appointment>> GetUpcomingByPatientAsync(int patientId, int count, CancellationToken cancellationToken = default);
}

public interface IDashboardRepository
{
    Task<int> GetTotalPatientsAsync(CancellationToken cancellationToken = default);
    Task<int> GetTotalDoctorsAsync(CancellationToken cancellationToken = default);
    Task<int> GetTodayAppointmentsCountAsync(CancellationToken cancellationToken = default);
    Task<int> GetPendingConfirmationsCountAsync(CancellationToken cancellationToken = default);
    Task<decimal> GetMonthlyRevenueAsync(int year, int month, CancellationToken cancellationToken = default);
    Task<decimal> GetPendingPaymentsTotalAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<(string Department, int Count)>> GetAppointmentsByDepartmentAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<(int DoctorId, string DoctorName, int Count)>> GetAppointmentsByDoctorAsync(CancellationToken cancellationToken = default);
    Task<int> GetDoctorTodayAppointmentsCountAsync(int doctorId, CancellationToken cancellationToken = default);
    Task<int> GetDoctorCompletedAppointmentsCountAsync(int doctorId, CancellationToken cancellationToken = default);
    Task<int> GetDoctorPatientCountAsync(int doctorId, CancellationToken cancellationToken = default);
}

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log, CancellationToken cancellationToken = default);
}

public interface IMedicalRecordRepository
{
    Task<MedicalRecord?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResult<MedicalRecord>> GetPagedAsync(PaginationQuery query, int? patientId = null, int? doctorId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MedicalRecord>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default);
}

public interface IPrescriptionRepository
{
    Task<Prescription?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResult<Prescription>> GetPagedAsync(PaginationQuery query, int? patientId = null, int? doctorId = null, CancellationToken cancellationToken = default);
}

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResult<Invoice>> GetPagedAsync(PaginationQuery query, int? patientId = null, string? paymentStatus = null, CancellationToken cancellationToken = default);
    Task<string> GenerateInvoiceNumberAsync(CancellationToken cancellationToken = default);
}

public interface IMedicineRepository
{
    Task<PagedResult<Medicine>> GetPagedAsync(PaginationQuery query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Medicine>> GetActiveMedicinesAsync(CancellationToken cancellationToken = default);
}

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByIdWithRolesAsync(int id, CancellationToken cancellationToken = default);
}

public interface IDepartmentRepository
{
    Task<IReadOnlyList<Department>> GetAllActiveAsync(CancellationToken cancellationToken = default);
}
