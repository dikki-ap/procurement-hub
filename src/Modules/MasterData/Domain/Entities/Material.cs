using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.MasterData.Domain.Entities;

public class Material : BaseSoftDeleteEntity
{
    public Guid     CategoryId     { get; set; }
    public string   Code           { get; set; } = string.Empty;
    public string   Name           { get; set; } = string.Empty;
    public string?  Description    { get; set; }
    public Guid     UomId          { get; set; }
    public decimal? EstimatedPrice { get; set; }
    public Guid?    CurrencyId     { get; set; }
    public bool     IsStrategic    { get; set; } = false;
    public bool     IsActive       { get; set; } = true;

    public MaterialCategory Category { get; set; } = null!;
    public UnitOfMeasure    Uom      { get; set; } = null!;
    public Currency?        Currency { get; set; }
}
