using ProcureHub.Modules.Procurement.Domain.Enums;

namespace ProcureHub.Modules.Procurement.Application.DTOs;

public record QuotationListDto(
    Guid            Id,
    Guid            RFQId,
    string          RFQNumber,
    string          RFQTitle,
    Guid            VendorId,
    string          VendorName,
    QuotationStatus Status,
    decimal         TotalPrice,
    DateTime        CreatedAt);
