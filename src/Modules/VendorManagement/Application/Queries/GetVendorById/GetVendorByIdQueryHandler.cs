using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.Modules.VendorManagement.Application.DTOs;
using ProcureHub.Modules.VendorManagement.Domain.Entities;
using ProcureHub.Modules.VendorManagement.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.VendorManagement.Application.Queries.GetVendorById;

public class GetVendorByIdQueryHandler : IQueryHandler<GetVendorByIdQuery, VendorDetailDto>
{
    private readonly IVendorRepository           _repo;
    private readonly IMaterialCategoryRepository _categoryRepo;
    private readonly ICacheService               _cache;

    public GetVendorByIdQueryHandler(
        IVendorRepository repo,
        IMaterialCategoryRepository categoryRepo,
        ICacheService cache)
    {
        _repo         = repo;
        _categoryRepo = categoryRepo;
        _cache        = cache;
    }

    public Task<VendorDetailDto> Handle(GetVendorByIdQuery query, CancellationToken ct)
        => _cache.GetOrSetAsync(
            CacheKeys.Vendors.ById(query.Id),
            async () =>
            {
                var v = await _repo.GetByIdWithDetailsAsync(query.Id, ct)
                    ?? throw new NotFoundException("Vendor", query.Id);

                var categoryIds = v.Capabilities.Select(c => c.MaterialCategoryId).Distinct();
                var categories  = await _categoryRepo.GetByIdsAsync(categoryIds, ct);
                var categoryMap = categories.ToDictionary(c => c.Id, c => c.Name);

                return ToDetailDto(v, categoryMap);
            },
            CacheTTL.VendorById);

    private static VendorDetailDto ToDetailDto(Vendor v, Dictionary<Guid, string> categoryMap) => new(
        v.Id,
        v.VendorCode,
        v.LegalName,
        v.TradeName,
        v.Npwp,
        v.Siup,
        v.Nib,
        v.Address,
        v.City,
        v.Province,
        v.PostalCode,
        v.Country,
        v.DefaultPaymentTermId,
        v.DefaultCurrencyId,
        v.VendorType,
        v.Status,
        v.Tier,
        v.Score,
        v.IsBlacklisted,
        v.BlacklistReason,
        v.ApprovedAt,
        v.CreatedAt,
        v.IsPkp,
        v.PphRate,
        v.Contacts.Select(c => new VendorContactDto(
            c.Id, c.Name, c.Position, c.Email, c.Phone, c.IsPrimary)).ToList(),
        v.Documents.Select(d => new VendorDocumentDto(
            d.Id, d.DocumentType, d.DocumentNumber, d.FileUrl, d.FileName,
            d.FileSize, d.ExpiredAt, d.IssuedAt, d.Status, d.Notes)).ToList(),
        v.Capabilities.Select(cap => new VendorCapabilityDto(
            cap.Id,
            cap.MaterialCategoryId,
            categoryMap.GetValueOrDefault(cap.MaterialCategoryId),
            cap.MinOrderQty,
            cap.MaxOrderQty,
            cap.Uom,
            cap.LeadTimeDays,
            cap.EffectiveDate,
            cap.ExpiryDate,
            cap.IsExpired,
            cap.Notes)).ToList(),
        v.BankAccounts.Select(b => new VendorBankAccountDto(
            b.Id, b.BankName, b.AccountNumber, b.AccountName, b.BranchName, b.Currency, b.IsDefault, b.Notes)).ToList());
}
