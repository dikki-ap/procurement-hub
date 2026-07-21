using ProcureHub.Modules.VendorManagement.Application.DTOs;
using ProcureHub.Modules.VendorManagement.Domain.Entities;
using ProcureHub.Modules.VendorManagement.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.VendorManagement.Application.Queries.GetVendorById;

public class GetVendorByIdQueryHandler : IQueryHandler<GetVendorByIdQuery, VendorDetailDto>
{
    private readonly IVendorRepository _repo;
    private readonly ICacheService     _cache;

    public GetVendorByIdQueryHandler(IVendorRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public Task<VendorDetailDto> Handle(GetVendorByIdQuery query, CancellationToken ct)
        => _cache.GetOrSetAsync(
            CacheKeys.Vendors.ById(query.Id),
            async () =>
            {
                var v = await _repo.GetByIdWithDetailsAsync(query.Id, ct)
                    ?? throw new NotFoundException("Vendor", query.Id);
                return ToDetailDto(v);
            },
            CacheTTL.VendorById);

    private static VendorDetailDto ToDetailDto(Vendor v) => new(
        v.Id,
        v.VendorCode,
        v.LegalName,
        v.TradeName,
        v.Npwp,
        v.Siup,
        v.Nib,
        v.VendorType,
        v.Status,
        v.Tier,
        v.Score,
        v.IsBlacklisted,
        v.BlacklistReason,
        v.ApprovedAt,
        v.CreatedAt,
        v.Contacts.Select(c => new VendorContactDto(
            c.Id, c.Name, c.Position, c.Email, c.Phone, c.IsPrimary)).ToList(),
        v.Documents.Select(d => new VendorDocumentDto(
            d.Id, d.DocumentType, d.DocumentNumber, d.FileUrl, d.FileName,
            d.FileSize, d.ExpiredAt, d.IssuedAt, d.Status, d.Notes)).ToList(),
        v.Capabilities.Select(cap => new VendorCapabilityDto(
            cap.Id, cap.MaterialCategoryId, cap.MinOrderQty, cap.Uom, cap.LeadTimeDays, cap.Notes)).ToList());
}
