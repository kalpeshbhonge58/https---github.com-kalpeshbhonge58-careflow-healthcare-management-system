using CareFlow.Application.Interfaces;
using CareFlow.Domain.Entities;
using CareFlow.Infrastructure.Data;
using CareFlow.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace CareFlow.Infrastructure.Repositories;

public class MedicineRepository : IMedicineRepository
{
    private readonly ApplicationDbContext _context;

    public MedicineRepository(ApplicationDbContext context) => _context = context;

    public async Task<PagedResult<Medicine>> GetPagedAsync(PaginationQuery query, CancellationToken cancellationToken = default)
    {
        var medicinesQuery = _context.Medicines
            .AsNoTracking()
            .Where(m => m.IsActive);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            medicinesQuery = medicinesQuery.Where(m =>
                m.Name.ToLower().Contains(search) ||
                (m.GenericName != null && m.GenericName.ToLower().Contains(search)) ||
                (m.Manufacturer != null && m.Manufacturer.ToLower().Contains(search)));
        }

        var descending = query.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);
        medicinesQuery = query.SortBy?.ToLower() switch
        {
            "price" => descending ? medicinesQuery.OrderByDescending(m => m.UnitPrice) : medicinesQuery.OrderBy(m => m.UnitPrice),
            "stock" => descending ? medicinesQuery.OrderByDescending(m => m.StockQuantity) : medicinesQuery.OrderBy(m => m.StockQuantity),
            _ => descending ? medicinesQuery.OrderByDescending(m => m.Name) : medicinesQuery.OrderBy(m => m.Name)
        };

        var totalRecords = await medicinesQuery.CountAsync(cancellationToken);
        var items = await medicinesQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Medicine>
        {
            Items = items,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalRecords = totalRecords
        };
    }

    public async Task<IReadOnlyList<Medicine>> GetActiveMedicinesAsync(CancellationToken cancellationToken = default) =>
        await _context.Medicines
            .AsNoTracking()
            .Where(m => m.IsActive && m.StockQuantity > 0)
            .OrderBy(m => m.Name)
            .ToListAsync(cancellationToken);
}
