using CareFlow.Application.DTOs.Doctor;
using CareFlow.Application.Interfaces;
using CareFlow.Shared.Constants;
using CareFlow.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorService _doctorService;
    private readonly ICurrentUserService _currentUser;

    public DoctorsController(IDoctorService doctorService, ICurrentUserService currentUser)
    {
        _doctorService = doctorService;
        _currentUser = currentUser;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PagedResult<DoctorResponse>>>> GetPaged(
        [FromQuery] PaginationQuery query, [FromQuery] int? departmentId, CancellationToken cancellationToken)
    {
        var result = await _doctorService.GetPagedAsync(query, departmentId, cancellationToken);
        return Ok(ApiResponse<PagedResult<DoctorResponse>>.Ok(result));
    }

    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DoctorResponse>>>> GetActive(CancellationToken cancellationToken)
    {
        var result = await _doctorService.GetActiveDoctorsAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<DoctorResponse>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<DoctorResponse>>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _doctorService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<DoctorResponse>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ApiResponse<DoctorResponse>>> Create(
        [FromBody] CreateDoctorRequest request, CancellationToken cancellationToken)
    {
        var result = await _doctorService.CreateAsync(request, _currentUser.UserId, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<DoctorResponse>.Ok(result, "Doctor created successfully."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Doctor}")]
    public async Task<ActionResult<ApiResponse<DoctorResponse>>> Update(
        int id, [FromBody] UpdateDoctorRequest request, CancellationToken cancellationToken)
    {
        var result = await _doctorService.UpdateAsync(id, request, _currentUser.UserId, cancellationToken);
        return Ok(ApiResponse<DoctorResponse>.Ok(result, "Doctor updated successfully."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ApiResponse>> Deactivate(int id, CancellationToken cancellationToken)
    {
        await _doctorService.DeactivateAsync(id, _currentUser.UserId, cancellationToken);
        return Ok(ApiResponse.Ok("Doctor deactivated successfully."));
    }
}
