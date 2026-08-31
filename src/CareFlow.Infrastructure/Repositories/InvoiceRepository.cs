using CareFlow.Application.Interfaces;
using CareFlow.Domain.Entities;
using CareFlow.Domain.Enums;
using CareFlow.Infrastructure.Data;
using CareFlow.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace CareFlow.Infrastructure.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly ApplicationDbContext _context;

    public InvoiceRepository(ApplicationDbContext context) => _context = context;

    public async Task<Invoice?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default) =>
        await _context.Invoices
            .AsNoTracking()
            .Include(i => i.Patient).ThenInclude(p => p.User)
            .Include(i => i.Appointment)
            .Include(i => i.Items)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public async Task<PagedResult<Invoice>> GetPagedAsync(PaginationQuery query, int? patientId = null, string? paymentStatus = null, CancellationToken cancellationToken = default)
    {
        var invoicesQuery = _context.Invoices
            .AsNoTracking()
            .Include(i => i.Patient).ThenInclude(p => p.User)
            .Include(i => i.Payments)
            .AsQueryable();

        if (patientId.HasValue)
            invoicesQuery = invoicesQuery.Where(i => i.PatientId == patientId.Value);

        if (!string.IsNullOrWhiteSpace(paymentStatus) && Enum.TryParse<PaymentStatus>(paymentStatus, true, out var status))
            invoicesQuery = invoicesQuery.Where(i => i.PaymentStatus == status);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            invoicesQuery = invoicesQuery.Where(i =>
                i.InvoiceNumber.ToLower().Contains(search) ||
                i.Patient.User.FirstName.ToLower().Contains(search) ||
                i.Patient.User.LastName.ToLower().Contains(search));
        }

        var descending = query.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);
        invoicesQuery = query.SortBy?.ToLower() switch
        {
            "total" => descending ? invoicesQuery.OrderByDescending(i => i.TotalAmount) : invoicesQuery.OrderBy(i => i.TotalAmount),
            "status" => descending ? invoicesQuery.OrderByDescending(i => i.PaymentStatus) : invoicesQuery.OrderBy(i => i.PaymentStatus),
            _ => descending ? invoicesQuery.OrderByDescending(i => i.InvoiceDate) : invoicesQuery.OrderBy(i => i.InvoiceDate)
        };

        var totalRecords = await invoicesQuery.CountAsync(cancellationToken);
        var items = await invoicesQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Invoice>
        {
            Items = items,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalRecords = totalRecords
        };
    }

    public async Task<string> GenerateInvoiceNumberAsync(CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"INV-{year}-";

        var lastNumber = await _context.Invoices
            .Where(i => i.InvoiceNumber.StartsWith(prefix))
            .OrderByDescending(i => i.InvoiceNumber)
            .Select(i => i.InvoiceNumber)
            .FirstOrDefaultAsync(cancellationToken);

        var sequence = 1;
        if (lastNumber is not null)
        {
            var numericPart = lastNumber[prefix.Length..];
            if (int.TryParse(numericPart, out var parsed))
                sequence = parsed + 1;
        }

        return $"{prefix}{sequence:D5}";
    }
}
