using ProcureHub.Modules.VendorManagement.Domain.Enums;
using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.VendorManagement.Domain.Entities;

public class VendorDocument : BaseSoftDeleteEntity
{
    public Guid           VendorId       { get; set; }
    public string         DocumentType   { get; set; } = string.Empty;
    public string?        DocumentNumber { get; set; }
    public string         FileUrl        { get; set; } = string.Empty;
    public string?        FileName       { get; set; }
    public long?          FileSize       { get; set; }
    public DateOnly?      ExpiredAt      { get; set; }
    public DateOnly?      IssuedAt       { get; set; }
    public DocumentStatus Status         { get; set; } = DocumentStatus.Active;
    public string?        Notes          { get; set; }

    public Vendor? Vendor { get; set; }
}
