using CareFlow.Application.DTOs.Billing;
using CareFlow.Application.Exceptions;
using CareFlow.Application.Interfaces;
using CareFlow.Application.Services;
using CareFlow.Domain.Entities;
using CareFlow.Domain.Enums;
using CareFlow.Shared.Constants;
using CareFlow.UnitTests.Helpers;
using Moq;

namespace CareFlow.UnitTests.Services;

public class InvoiceServiceTests
{
    private readonly Mock<IInvoiceRepository> _invoiceRepo = new();
    private readonly Mock<IRepository<Invoice>> _invoiceEntityRepo = new();
    private readonly Mock<IRepository<InvoiceItem>> _invoiceItemRepo = new();
    private readonly Mock<IRepository<Payment>> _paymentRepo = new();
    private readonly Mock<IRepository<Patient>> _patientRepo = new();
    private readonly Mock<IRepository<Appointment>> _appointmentRepo = new();
    private readonly Mock<IAuditService> _auditService = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly InvoiceService _sut;

    public InvoiceServiceTests()
    {
        _sut = new InvoiceService(
            _invoiceRepo.Object,
            _invoiceEntityRepo.Object,
            _invoiceItemRepo.Object,
            _paymentRepo.Object,
            _patientRepo.Object,
            _appointmentRepo.Object,
            _auditService.Object,
            _unitOfWork.Object,
            TestHelpers.CreateMapper());
    }

    [Fact]
    public async Task CreateAsync_CalculatesTaxAndTotalCorrectly()
    {
        var request = new CreateInvoiceRequest
        {
            PatientId = 1,
            InvoiceDate = DateTime.UtcNow.Date,
            DiscountAmount = 10m,
            Items =
            [
                new InvoiceItemRequest { Description = "Consultation", Quantity = 1, UnitPrice = 200m },
                new InvoiceItemRequest { Description = "Lab Test", Quantity = 2, UnitPrice = 50m }
            ]
        };

        var patient = TestHelpers.CreatePatient();
        var invoice = CreateInvoiceWithDetails(1, 300m, 30m, 10m, 320m);

        _patientRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(patient);
        _invoiceRepo.Setup(r => r.GenerateInvoiceNumberAsync(It.IsAny<CancellationToken>())).ReturnsAsync("INV-TEST-001");
        _invoiceEntityRepo.Setup(r => r.AddAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()))
            .Callback<Invoice, CancellationToken>((inv, _) =>
            {
                inv.Id = 1;
                Assert.Equal(300m, inv.SubTotal);
                Assert.Equal(30m, inv.TaxAmount);
                Assert.Equal(320m, inv.TotalAmount);
                Assert.Equal(PaymentStatus.Pending, inv.PaymentStatus);
            })
            .ReturnsAsync((Invoice inv, CancellationToken _) => inv);
        _invoiceItemRepo.Setup(r => r.AddAsync(It.IsAny<InvoiceItem>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((InvoiceItem item, CancellationToken _) => item);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _invoiceRepo.Setup(r => r.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);

        var result = await _sut.CreateAsync(request);

        Assert.Equal(300m, result.SubTotal);
        Assert.Equal(30m, result.TaxAmount);
        Assert.Equal(320m, result.TotalAmount);
        Assert.Equal("Pending", result.PaymentStatus);
    }

    [Fact]
    public async Task RecordPaymentAsync_FullPayment_SetsStatusToPaid()
    {
        var invoice = CreateInvoiceWithDetails(1, 100m, 10m, 0m, 110m);
        _invoiceRepo.Setup(r => r.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);
        _paymentRepo.Setup(r => r.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment p, CancellationToken _) => p);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var request = new CreatePaymentRequest
        {
            InvoiceId = 1,
            Amount = 110m,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = "Credit Card"
        };

        var result = await _sut.RecordPaymentAsync(request);

        Assert.Equal(110m, result.Amount);
        Assert.Equal(PaymentStatus.Paid, invoice.PaymentStatus);
    }

    [Fact]
    public async Task RecordPaymentAsync_PartialPayment_SetsStatusToPartiallyPaid()
    {
        var invoice = CreateInvoiceWithDetails(1, 200m, 20m, 0m, 220m);
        _invoiceRepo.Setup(r => r.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);
        _paymentRepo.Setup(r => r.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment p, CancellationToken _) => p);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var request = new CreatePaymentRequest
        {
            InvoiceId = 1,
            Amount = 100m,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = "Cash"
        };

        await _sut.RecordPaymentAsync(request);

        Assert.Equal(PaymentStatus.PartiallyPaid, invoice.PaymentStatus);
    }

    [Fact]
    public async Task RecordPaymentAsync_AlreadyPaid_ThrowsValidationException()
    {
        var invoice = CreateInvoiceWithDetails(1, 100m, 10m, 0m, 110m);
        invoice.PaymentStatus = PaymentStatus.Paid;
        invoice.Payments.Add(new Payment { Amount = 110m, PaymentDate = DateTime.UtcNow, PaymentMethod = "Card" });

        _invoiceRepo.Setup(r => r.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);

        var request = new CreatePaymentRequest { InvoiceId = 1, Amount = 10m, PaymentDate = DateTime.UtcNow, PaymentMethod = "Cash" };

        await Assert.ThrowsAsync<CareFlow.Application.Exceptions.ValidationException>(() => _sut.RecordPaymentAsync(request));
    }

    private static Invoice CreateInvoiceWithDetails(int id, decimal subTotal, decimal tax, decimal discount, decimal total)
    {
        var patient = TestHelpers.CreatePatient();
        return new Invoice
        {
            Id = id,
            InvoiceNumber = "INV-TEST",
            PatientId = patient.Id,
            Patient = patient,
            InvoiceDate = DateTime.UtcNow.Date,
            SubTotal = subTotal,
            TaxAmount = tax,
            DiscountAmount = discount,
            TotalAmount = total,
            PaymentStatus = PaymentStatus.Pending,
            Items = [new InvoiceItem { Description = "Service", Quantity = 1, UnitPrice = subTotal, TotalPrice = subTotal }],
            Payments = []
        };
    }
}
