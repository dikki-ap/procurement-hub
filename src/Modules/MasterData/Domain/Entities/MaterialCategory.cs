using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.MasterData.Domain.Entities;

public class MaterialCategory : BaseAuditableEntity
{
    public Guid   CompanyId   { get; set; }
    public string Name        { get; set; } = string.Empty;
    public string Code        { get; set; } = string.Empty;
    public Guid?  ParentId    { get; set; }
    public bool   IsStrategic { get; set; } = false;
    public bool   IsActive    { get; set; } = true;

    public MaterialCategory? Parent   { get; set; }
    public ICollection<MaterialCategory> Children { get; set; } = [];
    public ICollection<Material> Materials { get; set; } = [];
}
