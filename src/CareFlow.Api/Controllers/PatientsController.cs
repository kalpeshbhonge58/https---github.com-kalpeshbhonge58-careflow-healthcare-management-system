using CareFlow.Application.DTOs.Patient;
using CareFlow.Application.Interfaces;
using CareFlow.Shared.Constants;
using CareFlow.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;
    private readonly ICurrentUserService _currentUser;

    public PatientsController(IPatientService patientService, ICurrentUserService currentUser)
    {
        _patientService = patientService;
        _currentUser = currentUser;
    }

    [HttpGet]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Doctor},{Roles.Receptionist}")]
    public async Task<ActionResult<ApiResponse<PagedResult<PatientResponse>>>> GetPaged(
        [FromQuery] PaginationQuery query, CancellationToken cancellationToken)
    {
        var result = await _patientService.GetPagedAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<PatientResponse>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<PatientDetailResponse>>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _patientService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<PatientDetailResponse>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Receptionist}")]
    public async Task<ActionResult<ApiResponse<PatientResponse>>> Create(
        [FromBody] CreatePatientRequest request, CancellationToken cancellationToken)
    {
        var result = await _patientService.CreateAsync(request, _currentUser.UserId, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<PatientResponse>.Ok(result, "Patient created successfully."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Receptionist}")]
    public async Task<ActionResult<ApiResponse<PatientResponse>>> Update(
        int id, [FromBody] UpdatePatientRequest request, CancellationToken cancellationToken)
    {
        var result = await _patientService.UpdateAsync(id, request, _currentUser.UserId, cancellationToken);
        return Ok(ApiResponse<PatientResponse>.Ok(result, "Patient updated successfully."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ApiResponse>> Deactivate(int id, CancellationToken cancellationToken)
    {
        await _patientService.DeactivateAsync(id, _currentUser.UserId, cancellationToken);
        return Ok(ApiResponse.Ok("Patient deactivated successfully."));
    }
}
