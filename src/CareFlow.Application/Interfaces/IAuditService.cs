namespace CareFlow.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(int? userId, string action, string entityName, int? entityId = null, string? description = null, string? ipAddress = null, CancellationToken cancellationToken = default);
}
