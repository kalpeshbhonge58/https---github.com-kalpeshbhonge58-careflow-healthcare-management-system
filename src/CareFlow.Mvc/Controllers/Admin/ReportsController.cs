using CareFlow.Mvc.Models;
using CareFlow.Mvc.Services;
using CareFlow.Mvc.ViewModels.Reports;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CareFlow.Mvc.Controllers.Admin;

/// <summary>
/// Admin reporting controller. All data is retrieved from the CareFlow Web API via HTTP —
/// MVC does not contain business logic or direct database access.
/// </summary>
[Route("Admin/[controller]")]
public class ReportsController : Controller
{
    private readonly ICareFlowApiClient _apiClient;
    private readonly CareFlowApiSettings _apiSettings;

    public ReportsController(ICareFlowApiClient apiClient, IOptions<CareFlowApiSettings> apiSettings)
    {
        _apiClient = apiClient;
        _apiSettings = apiSettings.Value;
    }

    [HttpGet("")]
    [HttpGet("[action]")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var dashboard = await _apiClient.GetAdminDashboardAsync(cancellationToken)
            ?? new DashboardOverviewViewModel();

        ViewBag.ApiConfig = await BuildApiConfigAsync(cancellationToken);
        return View(dashboard);
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> Revenue(string? paymentStatus, CancellationToken cancellationToken)
    {
        var model = await _apiClient.GetRevenueReportAsync(paymentStatus, cancellationToken);
        ViewBag.ApiConfig = await BuildApiConfigAsync(cancellationToken);
        return View(model);
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> Appointments(string? status, int? doctorId, CancellationToken cancellationToken)
    {
        var model = await _apiClient.GetAppointmentsReportAsync(status, doctorId, cancellationToken);
        ViewBag.ApiConfig = await BuildApiConfigAsync(cancellationToken);
        return View(model);
    }

    private async Task<ReportsPageViewModel> BuildApiConfigAsync(CancellationToken cancellationToken)
    {
        var token = await _apiClient.GetAuthTokenAsync(cancellationToken);
        return new ReportsPageViewModel
        {
            ApiBaseUrl = _apiSettings.BaseUrl.TrimEnd('/'),
            ApiToken = token
        };
    }
}
