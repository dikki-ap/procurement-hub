using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProcureHub.SharedKernel.Abstractions;
using ProcureHub.SharedKernel.Audit;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.Modules.Audit.Application.Commands.ExportAuditLog;

public class ExportAuditLogCommandHandler : IRequestHandler<ExportAuditLogCommand, string>
{
    private const string Bucket = "audit-exports";

    private readonly ApplicationDbContext _db;
    private readonly IStorageService      _storage;

    public ExportAuditLogCommandHandler(ApplicationDbContext db, IStorageService storage)
    {
        _db      = db;
        _storage = storage;
    }

    public async Task<string> Handle(ExportAuditLogCommand request, CancellationToken ct)
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

        var rows = await query
            .OrderByDescending(a => a.CreatedAt)
            .Take(50_000)
            .ToListAsync(ct);

        var csv = BuildCsv(rows);

        await _storage.EnsureBucketExistsAsync(Bucket, ct);

        var objectKey = $"audit_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.csv";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        await _storage.UploadAsync(Bucket, objectKey, stream, "text/csv", ct);

        return await _storage.GetPresignedUrlAsync(Bucket, objectKey, TimeSpan.FromHours(1), ct: ct);
    }

    private static string BuildCsv(List<AuditLog> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Id,CreatedAt,EntityType,EntityId,Action,UserEmail,UserFullName,IpAddress,CorrelationId,ChangedColumns,BeforeValues,AfterValues");

        foreach (var r in rows)
        {
            sb.Append(r.Id).Append(',')
              .Append(r.CreatedAt.ToString("O")).Append(',')
              .Append(Escape(r.EntityType)).Append(',')
              .Append(r.EntityId).Append(',')
              .Append(Escape(r.Action)).Append(',')
              .Append(Escape(r.UserEmail)).Append(',')
              .Append(Escape(r.UserFullName)).Append(',')
              .Append(Escape(r.IpAddress)).Append(',')
              .Append(Escape(r.CorrelationId)).Append(',')
              .Append(Escape(r.ChangedColumns)).Append(',')
              .Append(Escape(r.BeforeValues)).Append(',')
              .AppendLine(Escape(r.AfterValues));
        }

        return sb.ToString();
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
