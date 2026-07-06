using ProcureHub.Modules.Procurement.Domain.Enums;

namespace ProcureHub.Modules.Procurement.Application.DTOs;

public class PRDto
{
    public Guid            Id                  { get; set; }
    public string          PRNumber            { get; set; } = string.Empty;
    public string          Title               { get; set; } = string.Empty;
    public string?         Description         { get; set; }
    public string          Department          { get; set; } = string.Empty;
    public string?         DeliveryLocation    { get; set; }
    public DateTime        RequiredDate        { get; set; }
    public PRStatus        Status              { get; set; }
    public decimal         TotalEstimatedValue { get; set; }
    public string?         Notes               { get; set; }
    public Guid            RequestedById       { get; set; }
    public DateTime        CreatedAt           { get; set; }
    public DateTime        UpdatedAt           { get; set; }
    public List<PRItemDto> Items               { get; set; } = [];
}
