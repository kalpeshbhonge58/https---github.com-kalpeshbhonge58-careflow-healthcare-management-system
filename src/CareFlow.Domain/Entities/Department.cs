using CareFlow.Domain.Common;

namespace CareFlow.Domain.Entities;

public class Department : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }

    public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
}
