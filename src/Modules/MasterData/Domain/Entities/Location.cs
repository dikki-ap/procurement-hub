using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.MasterData.Domain.Entities;

public class Location : BaseAuditableEntity
{
    public Guid    CompanyId { get; set; }
    public string  Name      { get; set; } = string.Empty;
    public string  Type      { get; set; } = string.Empty;
    public string? Address   { get; set; }
    public string? City      { get; set; }
    public string? Province  { get; set; }
    public string  Country   { get; set; } = "Indonesia";
    public bool    IsActive  { get; set; } = true;
}
