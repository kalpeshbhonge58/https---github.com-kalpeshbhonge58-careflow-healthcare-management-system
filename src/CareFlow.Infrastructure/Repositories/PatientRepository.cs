using CareFlow.Application.Interfaces;
using CareFlow.Domain.Entities;
using CareFlow.Infrastructure.Data;
using CareFlow.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace CareFlow.Infrastructure.Repositories;

public class PatientRepository : IPatientRepository
{
    private readonly ApplicationDbContext _context;

    public PatientRepository(ApplicationDbContext context) => _context = context;

    public async Task<Patient?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default) =>
        await _context.Patients
            .AsNoTracking()
            .Include(p => p.User)
            .Include(p => p.Appointments)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<Patient?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default) =>
        await _context.Patients
            .AsNoTracking()
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

    public async Task<PagedResult<Patient>> GetPagedAsync(PaginationQuery query, CancellationToken cancellationToken = default)
    {
        var patientsQuery = _context.Patients
            .AsNoTracking()
            .Include(p => p.User)
            .Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            patientsQuery = patientsQuery.Where(p =>
                p.PatientNumber.ToLower().Contains(search) ||
                p.User.FirstName.ToLower().Contains(search) ||
                p.User.LastName.ToLower().Contains(search) ||
                p.User.Email.ToLower().Contains(search));
        }

        patientsQuery = ApplySorting(patientsQuery, query);

        var totalRecords = await patientsQuery.CountAsync(cancellationToken);
        var items = await patientsQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Patient>
        {
            Items = items,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalRecords = totalRecords
        };
    }

    public async Task<bool> PatientNumberExistsAsync(string patientNumber, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Patients.Where(p => p.PatientNumber == patientNumber);
        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    private static IQueryable<Patient> ApplySorting(IQueryable<Patient> query, PaginationQuery pagination)
    {
        var descending = pagination.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);
        return pagination.SortBy?.ToLower() switch
        {
            "patientnumber" => descending ? query.OrderByDescending(p => p.PatientNumber) : query.OrderBy(p => p.PatientNumber),
            "dateofbirth" => descending ? query.OrderByDescending(p => p.DateOfBirth) : query.OrderBy(p => p.DateOfBirth),
            "email" => descending ? query.OrderByDescending(p => p.User.Email) : query.OrderBy(p => p.User.Email),
            _ => descending ? query.OrderByDescending(p => p.User.LastName) : query.OrderBy(p => p.User.LastName)
        };
    }
}
