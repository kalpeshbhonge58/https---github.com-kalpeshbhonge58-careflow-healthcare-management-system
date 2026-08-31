using CareFlow.Application.DTOs.MedicalRecord;
using CareFlow.Application.Interfaces;
using CareFlow.Shared.Constants;
using CareFlow.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MedicalRecordsController : ControllerBase
{
    private readonly IMedicalRecordService _service;
    private readonly ICurrentUserService _currentUser;

    public MedicalRecordsController(IMedicalRecordService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<MedicalRecordResponse>>>> GetPaged(
        [FromQuery] PaginationQuery query, [FromQuery] int? patientId, [FromQuery] int? doctorId, CancellationToken cancellationToken)
    {
        var result = await _service.GetPagedAsync(query, patientId, doctorId, cancellationToken);
        return Ok(ApiResponse<PagedResult<MedicalRecordResponse>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<MedicalRecordResponse>>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<MedicalRecordResponse>.Ok(result));
    }

    [HttpGet("patient/{patientId:int}")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MedicalRecordResponse>>>> GetByPatient(int patientId, CancellationToken cancellationToken)
    {
        var result = await _service.GetByPatientIdAsync(patientId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<MedicalRecordResponse>>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Doctor}")]
    public async Task<ActionResult<ApiResponse<MedicalRecordResponse>>> Create(
        [FromBody] CreateMedicalRecordRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(request, _currentUser.UserId, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<MedicalRecordResponse>.Ok(result, "Medical record created successfully."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Doctor}")]
    public async Task<ActionResult<ApiResponse<MedicalRecordResponse>>> Update(
        int id, [FromBody] UpdateMedicalRecordRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(id, request, _currentUser.UserId, cancellationToken);
        return Ok(ApiResponse<MedicalRecordResponse>.Ok(result, "Medical record updated successfully."));
    }
}
