using CareFlow.Application.Interfaces;
using CareFlow.Domain.Entities;

namespace CareFlow.Application.Services;

public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogRepository;

    public AuditService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task LogAsync(int? userId, string action, string entityName, int? entityId = null, string? description = null, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var log = new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            Description = description,
            IpAddress = ipAddress,
            Timestamp = DateTime.UtcNow
        };

        await _auditLogRepository.AddAsync(log, cancellationToken);
    }
}
