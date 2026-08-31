using CareFlow.Application.Interfaces;
using CareFlow.Domain.Entities;
using CareFlow.Infrastructure.Data;
using CareFlow.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace CareFlow.Infrastructure.Repositories;

public class DoctorRepository : IDoctorRepository
{
    private readonly ApplicationDbContext _context;

    public DoctorRepository(ApplicationDbContext context) => _context = context;

    public async Task<Doctor?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default) =>
        await _context.Doctors
            .AsNoTracking()
            .Include(d => d.User)
            .Include(d => d.Department)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public async Task<Doctor?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default) =>
        await _context.Doctors
            .AsNoTracking()
            .Include(d => d.User)
            .Include(d => d.Department)
            .FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);

    public async Task<PagedResult<Doctor>> GetPagedAsync(PaginationQuery query, int? departmentId = null, CancellationToken cancellationToken = default)
    {
        var doctorsQuery = _context.Doctors
            .AsNoTracking()
            .Include(d => d.User)
            .Include(d => d.Department)
            .Where(d => d.IsActive);

        if (departmentId.HasValue)
            doctorsQuery = doctorsQuery.Where(d => d.DepartmentId == departmentId.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            doctorsQuery = doctorsQuery.Where(d =>
                d.Specialization.ToLower().Contains(search) ||
                d.LicenseNumber.ToLower().Contains(search) ||
                d.User.FirstName.ToLower().Contains(search) ||
                d.User.LastName.ToLower().Contains(search) ||
                d.Department.Name.ToLower().Contains(search));
        }

        var descending = query.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);
        doctorsQuery = query.SortBy?.ToLower() switch
        {
            "specialization" => descending ? doctorsQuery.OrderByDescending(d => d.Specialization) : doctorsQuery.OrderBy(d => d.Specialization),
            "fee" => descending ? doctorsQuery.OrderByDescending(d => d.ConsultationFee) : doctorsQuery.OrderBy(d => d.ConsultationFee),
            "department" => descending ? doctorsQuery.OrderByDescending(d => d.Department.Name) : doctorsQuery.OrderBy(d => d.Department.Name),
            _ => descending ? doctorsQuery.OrderByDescending(d => d.User.LastName) : doctorsQuery.OrderBy(d => d.User.LastName)
        };

        var totalRecords = await doctorsQuery.CountAsync(cancellationToken);
        var items = await doctorsQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Doctor>
        {
            Items = items,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalRecords = totalRecords
        };
    }

    public async Task<IReadOnlyList<Doctor>> GetActiveDoctorsAsync(CancellationToken cancellationToken = default) =>
        await _context.Doctors
            .AsNoTracking()
            .Include(d => d.User)
            .Include(d => d.Department)
            .Where(d => d.IsActive)
            .OrderBy(d => d.User.LastName)
            .ToListAsync(cancellationToken);
}
