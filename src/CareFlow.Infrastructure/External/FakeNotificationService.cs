using CareFlow.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CareFlow.Infrastructure.External;

public class FakeNotificationService : IExternalNotificationService
{
    private readonly ILogger<FakeNotificationService> _logger;
    private readonly List<(string Recipient, string Subject, string Message, DateTime SentAt)> _sentEmails = [];

    public FakeNotificationService(ILogger<FakeNotificationService> logger) => _logger = logger;

    public IReadOnlyList<(string Recipient, string Subject, string Message, DateTime SentAt)> SentEmails => _sentEmails;

    public Task<bool> SendEmailAsync(string recipient, string subject, string message, CancellationToken cancellationToken = default)
    {
        _sentEmails.Add((recipient, subject, message, DateTime.UtcNow));
        _logger.LogInformation("[FAKE EMAIL] To: {Recipient} | Subject: {Subject}", recipient, subject);
        return Task.FromResult(true);
    }

    public void Clear() => _sentEmails.Clear();
}
