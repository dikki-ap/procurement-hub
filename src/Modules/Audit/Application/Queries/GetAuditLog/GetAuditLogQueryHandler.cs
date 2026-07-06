using MediatR;
using Microsoft.EntityFrameworkCore;
using ProcureHub.SharedKernel.Audit;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.Modules.Audit.Application.Queries.GetAuditLog;

public class GetAuditLogQueryHandler : IRequestHandler<GetAuditLogQuery, PagedAuditLogDto>
{
    private readonly ApplicationDbContext _db;

    public GetAuditLogQueryHandler(ApplicationDbContext db) => _db = db;

    public async Task<PagedAuditLogDto> Handle(GetAuditLogQuery request, CancellationToken ct)
    {
        var query = _db.Set<AuditLog>().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.EntityType))
            query = query.Where(a => a.EntityType == request.EntityType);

        if (request.EntityId.HasValue)
            query = query.Where(a => a.EntityId == request.EntityId.Value);

        if (request.UserId.HasValue)
            query = query.Where(a => a.UserId == request.UserId.Value);

        if (!string.IsNullOrWhiteSpace(request.Action))
            query = query.Where(a => a.Action == request.Action);

        if (request.From.HasValue)
            query = query.Where(a => a.CreatedAt >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(a => a.CreatedAt <= request.To.Value);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AuditLogDto(
                a.Id, a.UserId, a.UserEmail, a.UserFullName,
                a.EntityType, a.EntityId, a.Action,
                a.BeforeValues, a.AfterValues, a.ChangedColumns,
                a.IpAddress, a.CorrelationId, a.CreatedAt))
            .ToListAsync(ct);

        var totalPages = (int)Math.Ceiling(total / (double)request.PageSize);

        return new PagedAuditLogDto(items, total, request.Page, request.PageSize, totalPages);
    }
}
