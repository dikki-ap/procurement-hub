using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.ApprovalEngine.Domain.Entities;

public class ApproverMatrixEntry : BaseAuditableEntity
{
    public Guid   CompanyId     { get; set; }
    public string ReferenceType { get; set; } = string.Empty;  // "PR" | "PO" | "RFQ"
    public int    Level         { get; set; }
    public string Name          { get; set; } = string.Empty;
    public string Position      { get; set; } = string.Empty;
    public string Email         { get; set; } = string.Empty;
}
