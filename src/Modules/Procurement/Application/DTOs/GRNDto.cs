using ProcureHub.Modules.Procurement.Domain.Enums;

namespace ProcureHub.Modules.Procurement.Application.DTOs;

public record GRNListDto(
    Guid      Id,
    string    GRNNumber,
    Guid      POId,
    string    PONumber,
    GRNStatus Status,
    DateTime? ReceivedAt,
    DateTime  CreatedAt
);

public record GRNDto(
    Guid             Id,
    string           GRNNumber,
    Guid             POId,
    string           PONumber,
    GRNStatus        Status,
    Guid             ReceivedBy,
    DateTime?        ReceivedAt,
    string?          DeliveryNoteNo,
    string?          Notes,
    DateTime         CreatedAt,
    List<GRNItemDto> Items
);

public record GRNItemDto(
    Guid          Id,
    Guid          POItemId,
    string        Description,
    decimal       ReceivedQty,
    decimal       RejectedQty,
    QualityStatus QualityStatus,
    string?       RejectReason,
    string?       Notes
);
