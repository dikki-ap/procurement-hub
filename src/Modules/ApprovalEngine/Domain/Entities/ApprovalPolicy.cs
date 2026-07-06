using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.ApprovalEngine.Domain.Entities;

public class ApprovalPolicy : BaseAuditableEntity
{
    public Guid    CompanyId           { get; set; }
    public string  ReferenceType       { get; set; } = string.Empty; // "PR", "PO", etc.
    public string  Name                { get; set; } = string.Empty;
    public decimal MinValue            { get; set; }
    public decimal? MaxValue           { get; set; }
    public int     RequiredLevels      { get; set; } = 1;
    public bool    IsStrategicOverride  { get; set; }
    public bool    IsSingleSourceOverride { get; set; }
    public bool    IsActive            { get; set; } = true;
}
