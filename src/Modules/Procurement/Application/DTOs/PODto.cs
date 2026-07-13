using ProcureHub.Modules.Procurement.Domain.Enums;

namespace ProcureHub.Modules.Procurement.Application.DTOs;

public record POListDto(
    Guid      Id,
    string    PONumber,
    Guid      VendorId,
    string    VendorName,
    POStatus  Status,
    decimal   TotalAmount,
    DateTime? ExpectedDelivery,
    DateTime? IssuedAt,
    DateTime  CreatedAt,
    string?   CreatedByName,
    string?   UpdatedByName,
    DateTime  UpdatedAt
);

public record PODto(
    Guid             Id,
    string           PONumber,
    Guid?            RFQId,
    Guid             VendorId,
    string           VendorName,
    POStatus         Status,
    decimal          TotalAmount,
    Guid?            CurrencyId,
    string?          CurrencyCode,
    Guid?            PaymentTermId,
    string?          PaymentTermName,
    Guid?            DeliveryLocationId,
    string?          DeliveryLocation,
    DateTime?        ExpectedDelivery,
    DateTime?        ActualDelivery,
    string?          FileUrl,
    string?          Notes,
    string?          TermsConditions,
    DateTime?        IssuedAt,
    DateTime?        AcknowledgedAt,
    DateTime?        CompletedAt,
    DateTime?        CancelledAt,
    string?          CancelledReason,
    DateTime         CreatedAt,
    List<POItemDto>  Items
);

public record POItemDto(
    Guid    Id,
    Guid?   MaterialId,
    string  Description,
    decimal Quantity,
    string? UomCode,
    decimal UnitPrice,
    decimal TotalPrice,
    decimal ReceivedQty
);
