namespace CareFlow.Mvc.ViewModels.Reports;

public class DashboardOverviewViewModel
{
    public int TotalPatients { get; set; }
    public int TotalDoctors { get; set; }
    public int TodayAppointments { get; set; }
    public int PendingConfirmations { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public decimal PendingPayments { get; set; }
    public IList<DepartmentStatViewModel> AppointmentsByDepartment { get; set; } = new List<DepartmentStatViewModel>();
    public IList<DoctorStatViewModel> AppointmentsByDoctor { get; set; } = new List<DoctorStatViewModel>();
}

public class DepartmentStatViewModel
{
    public string Department { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class DoctorStatViewModel
{
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class RevenueReportViewModel
{
    public IList<InvoiceReportItemViewModel> Invoices { get; set; } = new List<InvoiceReportItemViewModel>();
    public string? SelectedPaymentStatus { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal PendingAmount { get; set; }
}

public class InvoiceReportItemViewModel
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
}

public class AppointmentsReportViewModel
{
    public IList<AppointmentReportItemViewModel> Appointments { get; set; } = new List<AppointmentReportItemViewModel>();
    public string? SelectedStatus { get; set; }
    public int? SelectedDoctorId { get; set; }
}

public class AppointmentReportItemViewModel
{
    public int Id { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string PatientNumber { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

public class ReportsPageViewModel
{
    public string ApiBaseUrl { get; set; } = string.Empty;
    public string ApiToken { get; set; } = string.Empty;
}
