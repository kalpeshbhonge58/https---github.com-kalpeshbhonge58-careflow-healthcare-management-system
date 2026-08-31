using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CareFlow.Mvc.Models;
using CareFlow.Mvc.ViewModels.Reports;
using CareFlow.Shared.Models;
using Microsoft.Extensions.Options;

namespace CareFlow.Mvc.Services;

/// <summary>
/// Calls the CareFlow Web API over HTTP. All reporting data is sourced from the API —
/// no business rules or data access are duplicated in the MVC project.
/// </summary>
public class CareFlowApiClient : ICareFlowApiClient
{
    private readonly HttpClient _httpClient;
    private readonly CareFlowApiSettings _settings;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
    private string? _cachedToken;
    private DateTime _tokenExpiresAt = DateTime.MinValue;

    public CareFlowApiClient(HttpClient httpClient, IOptions<CareFlowApiSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public async Task<string> GetAuthTokenAsync(CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiresAt)
            return _cachedToken;

        var loginPayload = new { email = _settings.AdminEmail, password = _settings.AdminPassword };
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginPayload, cancellationToken);
        response.EnsureSuccessStatusCode();

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<LoginApiResult>>(_jsonOptions, cancellationToken);
        _cachedToken = apiResponse?.Data?.Token
            ?? throw new InvalidOperationException("Failed to obtain API authentication token.");
        _tokenExpiresAt = apiResponse.Data.ExpiresAt.AddMinutes(-1);

        return _cachedToken;
    }

    public async Task<DashboardOverviewViewModel?> GetAdminDashboardAsync(CancellationToken cancellationToken = default)
    {
        var response = await SendAuthorizedGetAsync("api/dashboard/admin", cancellationToken);
        if (response is null) return null;

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<DashboardOverviewViewModel>>(_jsonOptions, cancellationToken);
        return apiResponse?.Data;
    }

    public async Task<RevenueReportViewModel> GetRevenueReportAsync(string? paymentStatus = null, CancellationToken cancellationToken = default)
    {
        var query = string.IsNullOrWhiteSpace(paymentStatus)
            ? "api/invoices?pageNumber=1&pageSize=100"
            : $"api/invoices?pageNumber=1&pageSize=100&paymentStatus={Uri.EscapeDataString(paymentStatus)}";

        var response = await SendAuthorizedGetAsync(query, cancellationToken);
        var invoices = new List<InvoiceReportItemViewModel>();

        if (response is not null)
        {
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<InvoiceReportItemViewModel>>>(_jsonOptions, cancellationToken);
            if (apiResponse?.Data?.Items is not null)
                invoices = apiResponse.Data.Items.ToList();
        }

        return new RevenueReportViewModel
        {
            Invoices = invoices,
            SelectedPaymentStatus = paymentStatus,
            TotalRevenue = invoices.Where(i => i.PaymentStatus is "Paid" or "PartiallyPaid").Sum(i => i.TotalAmount),
            PendingAmount = invoices.Where(i => i.PaymentStatus == "Pending").Sum(i => i.TotalAmount)
        };
    }

    public async Task<AppointmentsReportViewModel> GetAppointmentsReportAsync(string? status = null, int? doctorId = null, CancellationToken cancellationToken = default)
    {
        var queryParts = new List<string> { "pageNumber=1", "pageSize=100" };
        if (!string.IsNullOrWhiteSpace(status))
            queryParts.Add($"status={Uri.EscapeDataString(status)}");
        if (doctorId.HasValue)
            queryParts.Add($"doctorId={doctorId.Value}");

        var query = $"api/appointments?{string.Join("&", queryParts)}";
        var response = await SendAuthorizedGetAsync(query, cancellationToken);
        var appointments = new List<AppointmentReportItemViewModel>();

        if (response is not null)
        {
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<AppointmentReportItemViewModel>>>(_jsonOptions, cancellationToken);
            if (apiResponse?.Data?.Items is not null)
                appointments = apiResponse.Data.Items.ToList();
        }

        return new AppointmentsReportViewModel
        {
            Appointments = appointments,
            SelectedStatus = status,
            SelectedDoctorId = doctorId
        };
    }

    private async Task<HttpResponseMessage?> SendAuthorizedGetAsync(string requestUri, CancellationToken cancellationToken)
    {
        var token = await GetAuthTokenAsync(cancellationToken);
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode ? response : null;
    }

    private sealed class LoginApiResult
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
