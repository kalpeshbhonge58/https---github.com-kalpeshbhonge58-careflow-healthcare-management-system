namespace CareFlow.Application.Interfaces;

public interface IExternalNotificationService
{
    Task<bool> SendEmailAsync(string recipient, string subject, string message, CancellationToken cancellationToken = default);
}
