using ProcureHub.Modules.Procurement.Domain.Enums;

namespace ProcureHub.Modules.Procurement.Application.DTOs;

public record QuotationDto(
    Guid                   Id,
    Guid                   RFQId,
    string                 RFQNumber,
    string                 RFQTitle,
    Guid                   VendorId,
    string                 VendorName,
    QuotationStatus        Status,
    decimal                TotalPrice,
    string?                Notes,
    DateTime               CreatedAt,
    List<QuotationItemDto> Items);
