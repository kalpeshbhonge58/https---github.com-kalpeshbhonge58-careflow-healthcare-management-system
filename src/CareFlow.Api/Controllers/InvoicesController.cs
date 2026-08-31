using CareFlow.Application.DTOs.Billing;
using CareFlow.Application.Interfaces;
using CareFlow.Shared.Constants;
using CareFlow.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _service;
    private readonly ICurrentUserService _currentUser;

    public InvoicesController(IInvoiceService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Receptionist}")]
    public async Task<ActionResult<ApiResponse<PagedResult<InvoiceResponse>>>> GetPaged(
        [FromQuery] PaginationQuery query, [FromQuery] int? patientId, [FromQuery] string? paymentStatus, CancellationToken cancellationToken)
    {
        var result = await _service.GetPagedAsync(query, patientId, paymentStatus, cancellationToken);
        return Ok(ApiResponse<PagedResult<InvoiceResponse>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<InvoiceResponse>>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<InvoiceResponse>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Receptionist}")]
    public async Task<ActionResult<ApiResponse<InvoiceResponse>>> Create(
        [FromBody] CreateInvoiceRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(request, _currentUser.UserId, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<InvoiceResponse>.Ok(result, "Invoice created successfully."));
    }

    [HttpPost("payments")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Receptionist}")]
    public async Task<ActionResult<ApiResponse<PaymentResponse>>> RecordPayment(
        [FromBody] CreatePaymentRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.RecordPaymentAsync(request, _currentUser.UserId, cancellationToken);
        return Ok(ApiResponse<PaymentResponse>.Ok(result, "Payment recorded successfully."));
    }
}
