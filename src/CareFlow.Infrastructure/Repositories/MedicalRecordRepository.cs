using CareFlow.Application.Interfaces;
using CareFlow.Domain.Entities;
using CareFlow.Infrastructure.Data;
using CareFlow.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace CareFlow.Infrastructure.Repositories;

public class MedicalRecordRepository : IMedicalRecordRepository
{
    private readonly ApplicationDbContext _context;

    public MedicalRecordRepository(ApplicationDbContext context) => _context = context;

    public async Task<MedicalRecord?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default) =>
        await _context.MedicalRecords
            .AsNoTracking()
            .Include(m => m.Patient).ThenInclude(p => p.User)
            .Include(m => m.Doctor).ThenInclude(d => d.User)
            .Include(m => m.Appointment)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public async Task<PagedResult<MedicalRecord>> GetPagedAsync(PaginationQuery query, int? patientId = null, int? doctorId = null, CancellationToken cancellationToken = default)
    {
        var recordsQuery = _context.MedicalRecords
            .AsNoTracking()
            .Include(m => m.Patient).ThenInclude(p => p.User)
            .Include(m => m.Doctor).ThenInclude(d => d.User)
            .AsQueryable();

        if (patientId.HasValue)
            recordsQuery = recordsQuery.Where(m => m.PatientId == patientId.Value);

        if (doctorId.HasValue)
            recordsQuery = recordsQuery.Where(m => m.DoctorId == doctorId.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            recordsQuery = recordsQuery.Where(m =>
                m.Diagnosis.ToLower().Contains(search) ||
                (m.Symptoms != null && m.Symptoms.ToLower().Contains(search)) ||
                (m.Treatment != null && m.Treatment.ToLower().Contains(search)));
        }

        var descending = query.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);
        recordsQuery = descending
            ? recordsQuery.OrderByDescending(m => m.VisitDate)
            : recordsQuery.OrderBy(m => m.VisitDate);

        var totalRecords = await recordsQuery.CountAsync(cancellationToken);
        var items = await recordsQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<MedicalRecord>
        {
            Items = items,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalRecords = totalRecords
        };
    }

    public async Task<IReadOnlyList<MedicalRecord>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default) =>
        await _context.MedicalRecords
            .AsNoTracking()
            .Include(m => m.Doctor).ThenInclude(d => d.User)
            .Where(m => m.PatientId == patientId)
            .OrderByDescending(m => m.VisitDate)
            .ToListAsync(cancellationToken);
}
