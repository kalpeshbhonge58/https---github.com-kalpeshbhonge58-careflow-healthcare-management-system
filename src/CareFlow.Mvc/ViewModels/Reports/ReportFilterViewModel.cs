namespace CareFlow.Mvc.ViewModels.Reports;

public class ReportFilterViewModel
{
    public string FilterType { get; set; } = string.Empty;
    public string? SelectedPaymentStatus { get; set; }
    public string? SelectedStatus { get; set; }
    public int? SelectedDoctorId { get; set; }
}
