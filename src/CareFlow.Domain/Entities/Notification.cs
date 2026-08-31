using CareFlow.Domain.Common;
using CareFlow.Domain.Enums;

namespace CareFlow.Domain.Entities;

public class Notification : BaseEntity
{
    public int? UserId { get; set; }
    public User? User { get; set; }
    public NotificationType Type { get; set; }
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
    public string Recipient { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime? SentAt { get; set; }
    public string? ErrorMessage { get; set; }
}
