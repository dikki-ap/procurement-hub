using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.MasterData.Domain.Entities;

public class UnitOfMeasure : BaseAuditableEntity
{
    public Guid   CompanyId { get; set; }
    public string Code      { get; set; } = string.Empty;
    public string Name      { get; set; } = string.Empty;
    public bool   IsActive  { get; set; } = true;

    public ICollection<Material> Materials { get; set; } = [];
}
