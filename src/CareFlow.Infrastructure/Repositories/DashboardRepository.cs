using CareFlow.Application.Interfaces;
using CareFlow.Domain.Enums;
using CareFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareFlow.Infrastructure.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly ApplicationDbContext _context;

    public DashboardRepository(ApplicationDbContext context) => _context = context;

    public Task<int> GetTotalPatientsAsync(CancellationToken cancellationToken = default) =>
        _context.Patients.CountAsync(p => p.IsActive, cancellationToken);

    public Task<int> GetTotalDoctorsAsync(CancellationToken cancellationToken = default) =>
        _context.Doctors.CountAsync(d => d.IsActive, cancellationToken);

    public Task<int> GetTodayAppointmentsCountAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        return _context.Appointments.CountAsync(
            a => a.AppointmentDate.Date == today && a.Status != AppointmentStatus.Cancelled,
            cancellationToken);
    }

    public Task<int> GetPendingConfirmationsCountAsync(CancellationToken cancellationToken = default) =>
        _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Scheduled, cancellationToken);

    public async Task<decimal> GetMonthlyRevenueAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = startDate.AddMonths(1);

        return await _context.Payments
            .Where(p => p.PaymentDate >= startDate && p.PaymentDate < endDate)
            .SumAsync(p => p.Amount, cancellationToken);
    }

    public async Task<decimal> GetPendingPaymentsTotalAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Where(i => i.PaymentStatus == PaymentStatus.Pending || i.PaymentStatus == PaymentStatus.PartiallyPaid)
            .GroupJoin(
                _context.Payments,
                invoice => invoice.Id,
                payment => payment.InvoiceId,
                (invoice, payments) => new
                {
                    invoice.TotalAmount,
                    PaidAmount = payments.Sum(p => p.Amount)
                })
            .SumAsync(x => x.TotalAmount - x.PaidAmount, cancellationToken);
    }

    public async Task<IReadOnlyList<(string Department, int Count)>> GetAppointmentsByDepartmentAsync(CancellationToken cancellationToken = default)
    {
        var results = await _context.Appointments
            .Where(a => a.Status != AppointmentStatus.Cancelled)
            .Join(
                _context.Doctors,
                appointment => appointment.DoctorId,
                doctor => doctor.Id,
                (appointment, doctor) => new { appointment, doctor })
            .Join(
                _context.Departments,
                x => x.doctor.DepartmentId,
                department => department.Id,
                (x, department) => department.Name)
            .GroupBy(name => name)
            .Select(g => new { Department = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        return results.Select(x => (x.Department, x.Count)).ToList();
    }

    public async Task<IReadOnlyList<(int DoctorId, string DoctorName, int Count)>> GetAppointmentsByDoctorAsync(CancellationToken cancellationToken = default)
    {
        var results = await _context.Appointments
            .Where(a => a.Status != AppointmentStatus.Cancelled)
            .Join(
                _context.Doctors.Include(d => d.User),
                appointment => appointment.DoctorId,
                doctor => doctor.Id,
                (appointment, doctor) => new
                {
                    doctor.Id,
                    DoctorName = doctor.User.FirstName + " " + doctor.User.LastName
                })
            .GroupBy(x => new { x.Id, x.DoctorName })
            .Select(g => new
            {
                g.Key.Id,
                g.Key.DoctorName,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        return results.Select(x => (x.Id, x.DoctorName, x.Count)).ToList();
    }

    public Task<int> GetDoctorTodayAppointmentsCountAsync(int doctorId, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        return _context.Appointments.CountAsync(
            a => a.DoctorId == doctorId
                && a.AppointmentDate.Date == today
                && a.Status != AppointmentStatus.Cancelled,
            cancellationToken);
    }

    public Task<int> GetDoctorCompletedAppointmentsCountAsync(int doctorId, CancellationToken cancellationToken = default) =>
        _context.Appointments.CountAsync(
            a => a.DoctorId == doctorId && a.Status == AppointmentStatus.Completed,
            cancellationToken);

    public async Task<int> GetDoctorPatientCountAsync(int doctorId, CancellationToken cancellationToken = default) =>
        await _context.Appointments
            .Where(a => a.DoctorId == doctorId && a.Status != AppointmentStatus.Cancelled)
            .Select(a => a.PatientId)
            .Distinct()
            .CountAsync(cancellationToken);
}
