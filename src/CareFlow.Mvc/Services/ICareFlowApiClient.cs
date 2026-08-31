using CareFlow.Mvc.ViewModels.Reports;

namespace CareFlow.Mvc.Services;

/// <summary>
/// HTTP client facade for the CareFlow Web API.
/// MVC consumes the API instead of duplicating business logic in the presentation layer.
/// </summary>
public interface ICareFlowApiClient
{
    Task<string> GetAuthTokenAsync(CancellationToken cancellationToken = default);
    Task<DashboardOverviewViewModel?> GetAdminDashboardAsync(CancellationToken cancellationToken = default);
    Task<RevenueReportViewModel> GetRevenueReportAsync(string? paymentStatus = null, CancellationToken cancellationToken = default);
    Task<AppointmentsReportViewModel> GetAppointmentsReportAsync(string? status = null, int? doctorId = null, CancellationToken cancellationToken = default);
}
