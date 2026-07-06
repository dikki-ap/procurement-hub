namespace ProcureHub.Modules.Procurement.Application.DTOs;

public class RFQItemDto
{
    public Guid    Id              { get; set; }
    public Guid?   PRItemId        { get; set; }
    public Guid?   MaterialId      { get; set; }
    public string  ItemDescription { get; set; } = string.Empty;
    public decimal Quantity        { get; set; }
    public Guid?   UnitOfMeasureId { get; set; }
    public string? UnitLabel       { get; set; }
}
