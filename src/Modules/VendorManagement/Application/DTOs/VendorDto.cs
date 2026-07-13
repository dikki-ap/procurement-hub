using ProcureHub.Modules.VendorManagement.Domain.Enums;

namespace ProcureHub.Modules.VendorManagement.Application.DTOs;


public record VendorDto(
    Guid         Id,
    string       VendorCode,
    string       LegalName,
    string?      TradeName,
    string?      Npwp,
    string?      Siup,
    string?      Nib,
    VendorType   VendorType,
    VendorStatus Status,
    VendorTier   Tier,
    decimal      Score,
    bool         IsBlacklisted,
    string?      BlacklistReason,
    DateTime?    ApprovedAt,
    DateTime     CreatedAt,
    string?      CreatedByName,
    string?      UpdatedByName,
    DateTime     UpdatedAt
);

public record VendorDetailDto(
    Guid                       Id,
    string                     VendorCode,
    string                     LegalName,
    string?                    TradeName,
    string?                    Npwp,
    string?                    Siup,
    string?                    Nib,
    VendorType                 VendorType,
    VendorStatus               Status,
    VendorTier                 Tier,
    decimal                    Score,
    bool                       IsBlacklisted,
    string?                    BlacklistReason,
    DateTime?                  ApprovedAt,
    DateTime                   CreatedAt,
    List<VendorContactDto>     Contacts,
    List<VendorDocumentDto>    Documents,
    List<VendorCapabilityDto>  Capabilities
);

public record VendorContactDto(
    Guid    Id,
    string  Name,
    string? Position,
    string? Email,
    string? Phone,
    bool    IsPrimary
);

public record VendorDocumentDto(
    Guid           Id,
    string         DocumentType,
    string?        DocumentNumber,
    string         FileUrl,
    string?        FileName,
    long?          FileSize,
    DateOnly?      ExpiredAt,
    DateOnly?      IssuedAt,
    DocumentStatus Status,
    string?        Notes
);

public record VendorCapabilityDto(
    Guid     Id,
    Guid     MaterialCategoryId,
    decimal? MinOrderQty,
    int?     LeadTimeDays,
    string?  Notes
);
