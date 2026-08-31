using CareFlow.Application.DTOs.Billing;
using CareFlow.Shared.Models;

namespace CareFlow.Application.Interfaces;

public interface IInvoiceService
{
    Task<PagedResult<InvoiceResponse>> GetPagedAsync(PaginationQuery query, int? patientId = null, string? paymentStatus = null, CancellationToken cancellationToken = default);
    Task<InvoiceResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<InvoiceResponse> CreateAsync(CreateInvoiceRequest request, int? performedByUserId = null, CancellationToken cancellationToken = default);
    Task<PaymentResponse> RecordPaymentAsync(CreatePaymentRequest request, int? performedByUserId = null, CancellationToken cancellationToken = default);
}
