using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.Notifications.Domain;

public class InAppNotification : BaseEntity
{
    public Guid      UserId        { get; set; }
    /// <summary>Set for vendor user notifications; null for internal users.</summary>
    public Guid?     VendorUserId  { get; set; }
    public string    Title         { get; set; } = string.Empty;
    public string    Message       { get; set; } = string.Empty;
    public string?   Link          { get; set; }
    public bool      IsRead        { get; set; } = false;
    public DateTime  CreatedAt     { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt        { get; set; }

    public void MarkAsRead()
    {
        IsRead = true;
        ReadAt = DateTime.UtcNow;
    }
}
