using CareFlow.Application.Interfaces;
using CareFlow.Domain.Entities;
using CareFlow.Infrastructure.Data;
using CareFlow.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace CareFlow.Infrastructure.Repositories;

public class PrescriptionRepository : IPrescriptionRepository
{
    private readonly ApplicationDbContext _context;

    public PrescriptionRepository(ApplicationDbContext context) => _context = context;

    public async Task<Prescription?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default) =>
        await _context.Prescriptions
            .AsNoTracking()
            .Include(p => p.Patient).ThenInclude(pat => pat.User)
            .Include(p => p.Doctor).ThenInclude(doc => doc.User)
            .Include(p => p.MedicalRecord)
            .Include(p => p.Items).ThenInclude(i => i.Medicine)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<PagedResult<Prescription>> GetPagedAsync(PaginationQuery query, int? patientId = null, int? doctorId = null, CancellationToken cancellationToken = default)
    {
        var prescriptionsQuery = _context.Prescriptions
            .AsNoTracking()
            .Include(p => p.Patient).ThenInclude(pat => pat.User)
            .Include(p => p.Doctor).ThenInclude(doc => doc.User)
            .Include(p => p.Items)
            .AsQueryable();

        if (patientId.HasValue)
            prescriptionsQuery = prescriptionsQuery.Where(p => p.PatientId == patientId.Value);

        if (doctorId.HasValue)
            prescriptionsQuery = prescriptionsQuery.Where(p => p.DoctorId == doctorId.Value);

        var descending = query.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);
        prescriptionsQuery = descending
            ? prescriptionsQuery.OrderByDescending(p => p.PrescriptionDate)
            : prescriptionsQuery.OrderBy(p => p.PrescriptionDate);

        var totalRecords = await prescriptionsQuery.CountAsync(cancellationToken);
        var items = await prescriptionsQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Prescription>
        {
            Items = items,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalRecords = totalRecords
        };
    }
}
