using CareFlow.Application.Interfaces;
using CareFlow.Domain.Entities;
using CareFlow.Infrastructure.Data;

namespace CareFlow.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly ApplicationDbContext _context;

    public AuditLogRepository(ApplicationDbContext context) => _context = context;

    public async Task AddAsync(AuditLog log, CancellationToken cancellationToken = default)
    {
        log.Timestamp = DateTime.UtcNow;
        await _context.AuditLogs.AddAsync(log, cancellationToken);
    }
}
