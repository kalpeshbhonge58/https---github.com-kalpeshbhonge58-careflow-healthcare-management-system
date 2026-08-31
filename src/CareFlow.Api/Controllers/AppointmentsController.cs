using CareFlow.Application.DTOs.Appointment;
using CareFlow.Application.Interfaces;
using CareFlow.Shared.Constants;
using CareFlow.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly ICurrentUserService _currentUser;

    public AppointmentsController(IAppointmentService appointmentService, ICurrentUserService currentUser)
    {
        _appointmentService = appointmentService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<AppointmentResponse>>>> GetPaged(
        [FromQuery] PaginationQuery query,
        [FromQuery] int? patientId,
        [FromQuery] int? doctorId,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var result = await _appointmentService.GetPagedAsync(query, patientId, doctorId, status, cancellationToken);
        return Ok(ApiResponse<PagedResult<AppointmentResponse>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<AppointmentResponse>>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _appointmentService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<AppointmentResponse>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Receptionist},{Roles.Patient}")]
    public async Task<ActionResult<ApiResponse<AppointmentResponse>>> Create(
        [FromBody] CreateAppointmentRequest request, CancellationToken cancellationToken)
    {
        var result = await _appointmentService.CreateAsync(request, _currentUser.UserId, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<AppointmentResponse>.Ok(result, "Appointment booked successfully."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Receptionist},{Roles.Doctor}")]
    public async Task<ActionResult<ApiResponse<AppointmentResponse>>> Update(
        int id, [FromBody] UpdateAppointmentRequest request, CancellationToken cancellationToken)
    {
        var result = await _appointmentService.UpdateAsync(id, request, _currentUser.UserId, cancellationToken);
        return Ok(ApiResponse<AppointmentResponse>.Ok(result, "Appointment updated successfully."));
    }

    [HttpPut("{id:int}/reschedule")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Receptionist},{Roles.Patient}")]
    public async Task<ActionResult<ApiResponse<AppointmentResponse>>> Reschedule(
        int id, [FromBody] RescheduleAppointmentRequest request, CancellationToken cancellationToken)
    {
        var result = await _appointmentService.RescheduleAsync(id, request, _currentUser.UserId, cancellationToken);
        return Ok(ApiResponse<AppointmentResponse>.Ok(result, "Appointment rescheduled successfully."));
    }

    [HttpPut("{id:int}/confirm")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Receptionist},{Roles.Doctor}")]
    public async Task<ActionResult<ApiResponse<AppointmentResponse>>> Confirm(int id, CancellationToken cancellationToken)
    {
        var result = await _appointmentService.ConfirmAsync(id, _currentUser.UserId, cancellationToken);
        return Ok(ApiResponse<AppointmentResponse>.Ok(result, "Appointment confirmed successfully."));
    }

    [HttpPut("{id:int}/cancel")]
    public async Task<ActionResult<ApiResponse<AppointmentResponse>>> Cancel(
        int id, [FromBody] CancelAppointmentRequest request, CancellationToken cancellationToken)
    {
        var result = await _appointmentService.CancelAsync(id, request.CancellationReason, _currentUser.UserId, cancellationToken);
        return Ok(ApiResponse<AppointmentResponse>.Ok(result, "Appointment cancelled successfully."));
    }

    [HttpPut("{id:int}/complete")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Doctor}")]
    public async Task<ActionResult<ApiResponse<AppointmentResponse>>> Complete(int id, CancellationToken cancellationToken)
    {
        var result = await _appointmentService.CompleteAsync(id, _currentUser.UserId, cancellationToken);
        return Ok(ApiResponse<AppointmentResponse>.Ok(result, "Appointment completed successfully."));
    }
}

public class CancelAppointmentRequest
{
    public string CancellationReason { get; set; } = string.Empty;
}
