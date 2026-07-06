namespace ProcureHub.Modules.Procurement.Application.DTOs;

public record QuotationItemDto(
    Guid    Id,
    Guid    RFQItemId,
    string  ItemDescription,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    string? Notes);
