using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.MasterData.Domain.Entities;

public class PaymentTerm : BaseAuditableEntity
{
    public Guid    CompanyId   { get; set; }
    public string  Code        { get; set; } = string.Empty;
    public string  Name        { get; set; } = string.Empty;
    public int     Days        { get; set; } = 0;
    public string? Description { get; set; }
    public bool    IsActive    { get; set; } = true;
}
