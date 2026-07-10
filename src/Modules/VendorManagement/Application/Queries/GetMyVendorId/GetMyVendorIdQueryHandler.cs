using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.VendorManagement.Domain.Entities;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Database;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.VendorManagement.Application.Queries.GetMyVendorId;

public class GetMyVendorIdQueryHandler : IQueryHandler<GetMyVendorIdQuery, Guid>
{
    private readonly ApplicationDbContext _db;

    public GetMyVendorIdQueryHandler(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Guid> Handle(GetMyVendorIdQuery query, CancellationToken ct)
    {
        var vendorUser = await _db.Set<VendorUser>()
            .FirstOrDefaultAsync(u => u.KeycloakId == query.KeycloakId, ct);

        if (vendorUser is null)
            throw new NotFoundException("VendorUser", query.KeycloakId);

        return vendorUser.VendorId;
    }
}
