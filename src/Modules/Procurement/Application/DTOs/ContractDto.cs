using ProcureHub.Modules.Procurement.Domain.Enums;

namespace ProcureHub.Modules.Procurement.Application.DTOs;

public record ContractListDto(
    Guid           Id,
    string         ContractNumber,
    string         Title,
    Guid           VendorId,
    string         VendorName,
    Guid?          POId,
    string?        PONumber,
    ContractStatus Status,
    DateTime?      StartDate,
    DateTime?      EndDate,
    decimal?       Value,
    Guid?          CurrencyId,
    DateTime       CreatedAt
);

public record ContractDto(
    Guid           Id,
    Guid           CompanyId,
    string         ContractNumber,
    string         Title,
    Guid           VendorId,
    string         VendorName,
    Guid?          POId,
    string?        PONumber,
    ContractStatus Status,
    bool           HasFile,
    DateTime?      SignedAt,
    DateTime?      StartDate,
    DateTime?      EndDate,
    decimal?       Value,
    Guid?          CurrencyId,
    string?        Notes,
    DateTime       CreatedAt,
    DateTime       UpdatedAt,
    string?        CreatedByName,
    string?        UpdatedByName
);
