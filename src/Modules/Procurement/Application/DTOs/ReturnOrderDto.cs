using ProcureHub.Modules.Procurement.Domain.Enums;

namespace ProcureHub.Modules.Procurement.Application.DTOs;

public record ReturnOrderListDto(
    Guid         Id,
    string       ReturnNumber,
    Guid         GRNId,
    Guid         POId,
    Guid         VendorId,
    string?      VendorName,
    ReturnStatus Status,
    string?      Reason,
    DateTime     CreatedAt,
    DateTime?    AcknowledgedAt,
    DateTime?    ShippedAt,
    DateTime?    ReceivedAt
);

public record ReturnOrderDto(
    Guid                     Id,
    string                   ReturnNumber,
    Guid                     GRNId,
    Guid                     POId,
    Guid                     VendorId,
    string?                  VendorName,
    ReturnStatus             Status,
    string?                  Reason,
    string?                  Notes,
    DateTime                 CreatedAt,
    DateTime?                AcknowledgedAt,
    DateTime?                ShippedAt,
    DateTime?                ReceivedAt,
    List<ReturnOrderItemDto> Items
);

public record ReturnOrderItemDto(
    Guid    Id,
    Guid?   POItemId,
    string  ItemDescription,
    decimal Quantity,
    string  Uom,
    string? ReturnReason
);
