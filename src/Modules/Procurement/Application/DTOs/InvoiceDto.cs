using ProcureHub.Modules.Procurement.Domain.Enums;

namespace ProcureHub.Modules.Procurement.Application.DTOs;

public record InvoiceListDto(
    Guid          Id,
    string        InvoiceNumber,
    Guid          POId,
    string        PONumber,
    Guid          VendorId,
    string        VendorName,
    InvoiceStatus Status,
    decimal       TotalAmount,
    DateTime?     DueAt,
    DateTime      SubmittedAt,
    DateTime      CreatedAt,
    string?       CreatedByName,
    string?       UpdatedByName,
    DateTime      UpdatedAt
);

public record InvoiceDto(
    Guid          Id,
    string        InvoiceNumber,
    Guid          POId,
    string        PONumber,
    Guid          VendorId,
    string        VendorName,
    InvoiceStatus Status,
    decimal       Amount,
    decimal       TaxAmount,
    decimal       TotalAmount,
    string?       CurrencyCode,
    string?       FileUrl,
    DateTime?     DueAt,
    DateTime?     PaidAt,
    string?       PaymentReference,
    string?       Notes,
    string?       RejectionReason,
    DateTime      SubmittedAt,
    DateTime?     ReviewedAt
);
