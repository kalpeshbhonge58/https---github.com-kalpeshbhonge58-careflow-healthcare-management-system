namespace CareFlow.Mvc.Models;

public class CareFlowApiSettings
{
    public const string SectionName = "CareFlowApi";

    public string BaseUrl { get; set; } = "http://localhost:5000";
    public string AdminEmail { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
}
