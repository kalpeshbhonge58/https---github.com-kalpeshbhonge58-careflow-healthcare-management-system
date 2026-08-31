using CareFlow.Application.Interfaces;
using CareFlow.Domain.Entities;
using CareFlow.Domain.Enums;
using CareFlow.Infrastructure.Data;
using CareFlow.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace CareFlow.Infrastructure.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly ApplicationDbContext _context;

    public AppointmentRepository(ApplicationDbContext context) => _context = context;

    public async Task<Appointment?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default) =>
        await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Doctor).ThenInclude(d => d.Department)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<PagedResult<Appointment>> GetPagedAsync(PaginationQuery query, int? patientId = null, int? doctorId = null, string? status = null, CancellationToken cancellationToken = default)
    {
        var appointmentsQuery = _context.Appointments
            .AsNoTracking()
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Doctor).ThenInclude(d => d.Department)
            .AsQueryable();

        if (patientId.HasValue)
            appointmentsQuery = appointmentsQuery.Where(a => a.PatientId == patientId.Value);

        if (doctorId.HasValue)
            appointmentsQuery = appointmentsQuery.Where(a => a.DoctorId == doctorId.Value);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<AppointmentStatus>(status, true, out var statusEnum))
            appointmentsQuery = appointmentsQuery.Where(a => a.Status == statusEnum);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            appointmentsQuery = appointmentsQuery.Where(a =>
                (a.Reason != null && a.Reason.ToLower().Contains(search)) ||
                a.Patient.User.FirstName.ToLower().Contains(search) ||
                a.Patient.User.LastName.ToLower().Contains(search) ||
                a.Doctor.User.FirstName.ToLower().Contains(search) ||
                a.Doctor.User.LastName.ToLower().Contains(search));
        }

        var descending = query.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);
        appointmentsQuery = query.SortBy?.ToLower() switch
        {
            "status" => descending ? appointmentsQuery.OrderByDescending(a => a.Status) : appointmentsQuery.OrderBy(a => a.Status),
            "doctor" => descending ? appointmentsQuery.OrderByDescending(a => a.Doctor.User.LastName) : appointmentsQuery.OrderBy(a => a.Doctor.User.LastName),
            "patient" => descending ? appointmentsQuery.OrderByDescending(a => a.Patient.User.LastName) : appointmentsQuery.OrderBy(a => a.Patient.User.LastName),
            _ => descending
                ? appointmentsQuery.OrderByDescending(a => a.AppointmentDate).ThenByDescending(a => a.StartTime)
                : appointmentsQuery.OrderBy(a => a.AppointmentDate).ThenBy(a => a.StartTime)
        };

        var totalRecords = await appointmentsQuery.CountAsync(cancellationToken);
        var items = await appointmentsQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Appointment>
        {
            Items = items,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalRecords = totalRecords
        };
    }

    public async Task<bool> HasDoctorConflictAsync(int doctorId, DateTime date, TimeSpan start, TimeSpan end, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var appointmentDate = date.Date;

        var query = _context.Appointments
            .Where(a => a.DoctorId == doctorId
                && a.AppointmentDate.Date == appointmentDate
                && a.Status != AppointmentStatus.Cancelled
                && a.Status != AppointmentStatus.NoShow
                && a.StartTime < end
                && a.EndTime > start);

        if (excludeId.HasValue)
            query = query.Where(a => a.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> HasPatientConflictAsync(int patientId, DateTime date, TimeSpan start, TimeSpan end, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var appointmentDate = date.Date;

        var query = _context.Appointments
            .Where(a => a.PatientId == patientId
                && a.AppointmentDate.Date == appointmentDate
                && a.Status != AppointmentStatus.Cancelled
                && a.Status != AppointmentStatus.NoShow
                && a.StartTime < end
                && a.EndTime > start);

        if (excludeId.HasValue)
            query = query.Where(a => a.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Appointment>> GetUpcomingByDoctorAsync(int doctorId, int count, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;

        return await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .Where(a => a.DoctorId == doctorId
                && a.AppointmentDate.Date >= today
                && a.Status != AppointmentStatus.Cancelled
                && a.Status != AppointmentStatus.Completed)
            .OrderBy(a => a.AppointmentDate)
            .ThenBy(a => a.StartTime)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Appointment>> GetUpcomingByPatientAsync(int patientId, int count, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;

        return await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Doctor).ThenInclude(d => d.Department)
            .Where(a => a.PatientId == patientId
                && a.AppointmentDate.Date >= today
                && a.Status != AppointmentStatus.Cancelled
                && a.Status != AppointmentStatus.Completed)
            .OrderBy(a => a.AppointmentDate)
            .ThenBy(a => a.StartTime)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
}
