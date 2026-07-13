using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.MasterData.Domain.Entities;

public class Currency : BaseAuditableEntity
{
    public string  Code         { get; set; } = string.Empty;
    public string  Name         { get; set; } = string.Empty;
    public string? Symbol       { get; set; }
    public decimal ExchangeRate { get; set; }
    public bool      IsBase        { get; set; } = false;
    public bool      IsActive      { get; set; } = true;
    public DateTime? RateUpdatedAt { get; set; }

    public ICollection<Material> Materials { get; set; } = [];
}
