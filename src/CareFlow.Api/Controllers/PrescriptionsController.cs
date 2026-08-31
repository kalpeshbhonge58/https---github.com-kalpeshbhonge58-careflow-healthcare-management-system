using CareFlow.Application.DTOs.Prescription;
using CareFlow.Application.Interfaces;
using CareFlow.Shared.Constants;
using CareFlow.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PrescriptionsController : ControllerBase
{
    private readonly IPrescriptionService _service;
    private readonly ICurrentUserService _currentUser;

    public PrescriptionsController(IPrescriptionService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<PrescriptionResponse>>>> GetPaged(
        [FromQuery] PaginationQuery query, [FromQuery] int? patientId, [FromQuery] int? doctorId, CancellationToken cancellationToken)
    {
        var result = await _service.GetPagedAsync(query, patientId, doctorId, cancellationToken);
        return Ok(ApiResponse<PagedResult<PrescriptionResponse>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<PrescriptionResponse>>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<PrescriptionResponse>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Doctor}")]
    public async Task<ActionResult<ApiResponse<PrescriptionResponse>>> Create(
        [FromBody] CreatePrescriptionRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(request, _currentUser.UserId, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<PrescriptionResponse>.Ok(result, "Prescription saved successfully."));
    }
}
