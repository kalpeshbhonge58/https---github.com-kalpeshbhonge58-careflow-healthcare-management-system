using System.Net.Http.Headers;
using System.Net.Http.Json;
using CareFlow.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CareFlow.Infrastructure.External;

public class ResendSettings
{
    public const string SectionName = "Resend";
    public string ApiKey { get; set; } = string.Empty;
    public string FromEmail { get; set; } = "CareFlow <noreply@careflow.com>";
    public string ApiUrl { get; set; } = "https://api.resend.com/emails";
    public bool UseMockFallback { get; set; } = true;
}

public class ResendNotificationService : IExternalNotificationService
{
    private readonly HttpClient _httpClient;
    private readonly ResendSettings _settings;
    private readonly ILogger<ResendNotificationService> _logger;

    public ResendNotificationService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<ResendNotificationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _settings = configuration.GetSection(ResendSettings.SectionName).Get<ResendSettings>() ?? new ResendSettings();
    }

    public async Task<bool> SendEmailAsync(string recipient, string subject, string message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            if (_settings.UseMockFallback)
            {
                _logger.LogWarning("Resend API key not configured. Using mock fallback for email to {Recipient}", recipient);
                return await MockSendAsync(recipient, subject, message);
            }

            _logger.LogError("Resend API key not configured and mock fallback is disabled.");
            return false;
        }

        try
        {
            var payload = new
            {
                from = _settings.FromEmail,
                to = new[] { recipient },
                subject,
                html = message
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, _settings.ApiUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
            request.Content = JsonContent.Create(payload);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email sent successfully to {Recipient}", recipient);
                return true;
            }

            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Resend API returned {StatusCode}: {Body}", response.StatusCode, errorBody);

            if (_settings.UseMockFallback)
            {
                _logger.LogWarning("Falling back to mock email delivery for {Recipient}", recipient);
                return await MockSendAsync(recipient, subject, message);
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email via Resend to {Recipient}", recipient);

            if (_settings.UseMockFallback)
            {
                _logger.LogWarning("Falling back to mock email delivery for {Recipient}", recipient);
                return await MockSendAsync(recipient, subject, message);
            }

            return false;
        }
    }

    private Task<bool> MockSendAsync(string recipient, string subject, string message)
    {
        _logger.LogInformation(
            "[MOCK EMAIL] To: {Recipient} | Subject: {Subject} | Message: {Message}",
            recipient,
            subject,
            message.Length > 100 ? message[..100] + "..." : message);

        return Task.FromResult(true);
    }
}
