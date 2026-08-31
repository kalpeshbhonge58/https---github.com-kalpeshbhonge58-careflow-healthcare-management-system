using AutoMapper;
using CareFlow.Application.DTOs.Billing;
using CareFlow.Application.Exceptions;
using CareFlow.Application.Interfaces;
using CareFlow.Domain.Entities;
using CareFlow.Domain.Enums;
using CareFlow.Shared.Constants;
using CareFlow.Shared.Models;

namespace CareFlow.Application.Services;

public class InvoiceService : IInvoiceService
{
    private const decimal TaxRate = 0.10m;

    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IRepository<Invoice> _invoiceEntityRepository;
    private readonly IRepository<InvoiceItem> _invoiceItemRepository;
    private readonly IRepository<Payment> _paymentRepository;
    private readonly IRepository<Patient> _patientRepository;
    private readonly IRepository<Appointment> _appointmentRepository;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public InvoiceService(
        IInvoiceRepository invoiceRepository,
        IRepository<Invoice> invoiceEntityRepository,
        IRepository<InvoiceItem> invoiceItemRepository,
        IRepository<Payment> paymentRepository,
        IRepository<Patient> patientRepository,
        IRepository<Appointment> appointmentRepository,
        IAuditService auditService,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _invoiceRepository = invoiceRepository;
        _invoiceEntityRepository = invoiceEntityRepository;
        _invoiceItemRepository = invoiceItemRepository;
        _paymentRepository = paymentRepository;
        _patientRepository = patientRepository;
        _appointmentRepository = appointmentRepository;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<InvoiceResponse>> GetPagedAsync(PaginationQuery query, int? patientId = null, string? paymentStatus = null, CancellationToken cancellationToken = default)
    {
        var paged = await _invoiceRepository.GetPagedAsync(query, patientId, paymentStatus, cancellationToken);
        return new PagedResult<InvoiceResponse>
        {
            Items = _mapper.Map<IEnumerable<InvoiceResponse>>(paged.Items),
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize,
            TotalRecords = paged.TotalRecords
        };
    }

    public async Task<InvoiceResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var invoice = await _invoiceRepository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Invoice), id);

        return _mapper.Map<InvoiceResponse>(invoice);
    }

    public async Task<InvoiceResponse> CreateAsync(CreateInvoiceRequest request, int? performedByUserId = null, CancellationToken cancellationToken = default)
    {
        if (request.Items.Count == 0)
            throw new ValidationException("At least one invoice item is required.");

        if (await _patientRepository.GetByIdAsync(request.PatientId, cancellationToken) is null)
            throw new NotFoundException(nameof(Patient), request.PatientId);

        if (request.AppointmentId.HasValue &&
            await _appointmentRepository.GetByIdAsync(request.AppointmentId.Value, cancellationToken) is null)
            throw new NotFoundException(nameof(Appointment), request.AppointmentId.Value);

        if (request.DiscountAmount < 0)
            throw new ValidationException("Discount amount cannot be negative.");

        var subTotal = request.Items.Sum(i => i.Quantity * i.UnitPrice);
        var taxAmount = Math.Round(subTotal * TaxRate, 2);
        var totalAmount = Math.Round(subTotal + taxAmount - request.DiscountAmount, 2);

        if (totalAmount < 0)
            throw new ValidationException("Total amount cannot be negative after discount.");

        var invoice = new Invoice
        {
            InvoiceNumber = await _invoiceRepository.GenerateInvoiceNumberAsync(cancellationToken),
            PatientId = request.PatientId,
            AppointmentId = request.AppointmentId,
            InvoiceDate = request.InvoiceDate,
            SubTotal = subTotal,
            TaxAmount = taxAmount,
            DiscountAmount = request.DiscountAmount,
            TotalAmount = totalAmount,
            PaymentStatus = PaymentStatus.Pending,
            Notes = request.Notes?.Trim()
        };

        await _invoiceEntityRepository.AddAsync(invoice, cancellationToken);

        foreach (var itemRequest in request.Items)
        {
            var item = new InvoiceItem
            {
                Invoice = invoice,
                Description = itemRequest.Description.Trim(),
                Quantity = itemRequest.Quantity,
                UnitPrice = itemRequest.UnitPrice,
                TotalPrice = itemRequest.Quantity * itemRequest.UnitPrice
            };
            await _invoiceItemRepository.AddAsync(item, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync(performedByUserId, AuditActions.InvoiceCreated, nameof(Invoice), invoice.Id, "Invoice created.", cancellationToken: cancellationToken);

        var created = await _invoiceRepository.GetByIdWithDetailsAsync(invoice.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Invoice), invoice.Id);

        return _mapper.Map<InvoiceResponse>(created);
    }

    public async Task<PaymentResponse> RecordPaymentAsync(CreatePaymentRequest request, int? performedByUserId = null, CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0)
            throw new ValidationException("Payment amount must be greater than zero.");

        var invoice = await _invoiceRepository.GetByIdWithDetailsAsync(request.InvoiceId, cancellationToken)
            ?? throw new NotFoundException(nameof(Invoice), request.InvoiceId);

        if (invoice.PaymentStatus == PaymentStatus.Cancelled)
            throw new ValidationException("Cannot record payment for a cancelled invoice.");

        if (invoice.PaymentStatus == PaymentStatus.Paid)
            throw new ValidationException("Invoice is already fully paid.");

        var totalPaid = invoice.Payments.Sum(p => p.Amount);
        var remaining = invoice.TotalAmount - totalPaid;

        if (request.Amount > remaining)
            throw new ValidationException($"Payment amount exceeds remaining balance of {remaining:C}.");

        var payment = new Payment
        {
            InvoiceId = invoice.Id,
            Amount = request.Amount,
            PaymentDate = request.PaymentDate,
            PaymentMethod = request.PaymentMethod.Trim(),
            TransactionReference = request.TransactionReference?.Trim(),
            Notes = request.Notes?.Trim()
        };

        await _paymentRepository.AddAsync(payment, cancellationToken);

        totalPaid += request.Amount;
        invoice.PaymentStatus = totalPaid >= invoice.TotalAmount
            ? PaymentStatus.Paid
            : PaymentStatus.PartiallyPaid;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync(performedByUserId, AuditActions.PaymentRecorded, nameof(Payment), payment.Id, "Payment recorded.", cancellationToken: cancellationToken);

        return _mapper.Map<PaymentResponse>(payment);
    }
}
