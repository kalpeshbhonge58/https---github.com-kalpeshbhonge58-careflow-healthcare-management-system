using CareFlow.Application.Interfaces;
using CareFlow.Shared.Constants;
using CareFlow.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService) => _dashboardService = dashboardService;

    [HttpGet("admin")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GetAdminDashboard(CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetAdminDashboardAsync(cancellationToken);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("doctor/{doctorId:int}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Doctor}")]
    public async Task<IActionResult> GetDoctorDashboard(int doctorId, CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetDoctorDashboardAsync(doctorId, cancellationToken);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("receptionist")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Receptionist}")]
    public async Task<IActionResult> GetReceptionistDashboard(CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetReceptionistDashboardAsync(cancellationToken);
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("patient/{patientId:int}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Patient},{Roles.Doctor}")]
    public async Task<IActionResult> GetPatientDashboard(int patientId, CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetPatientDashboardAsync(patientId, cancellationToken);
        return Ok(ApiResponse<object>.Ok(result));
    }
}
