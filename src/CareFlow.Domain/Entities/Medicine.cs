using CareFlow.Domain.Common;

namespace CareFlow.Domain.Entities;

public class Medicine : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public string? Manufacturer { get; set; }
    public string? Description { get; set; }
    public string? DosageForm { get; set; }
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }

    public ICollection<PrescriptionItem> PrescriptionItems { get; set; } = new List<PrescriptionItem>();
}
