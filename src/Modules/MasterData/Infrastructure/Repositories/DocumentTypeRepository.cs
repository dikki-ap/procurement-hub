using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.MasterData.Domain.Entities;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.Modules.MasterData.Infrastructure.Repositories;

public class DocumentTypeRepository : IDocumentTypeRepository
{
    private readonly ApplicationDbContext _db;

    public DocumentTypeRepository(ApplicationDbContext db) => _db = db;

    public Task<List<DocumentType>> GetAllAsync(CancellationToken ct = default)
        => _db.Set<DocumentType>()
              .OrderBy(e => e.Name)
              .ToListAsync(ct);

    public Task<DocumentType?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Set<DocumentType>().FirstOrDefaultAsync(e => e.Id == id, ct);

    public Task<DocumentType?> GetByNameAsync(string name, CancellationToken ct = default)
        => _db.Set<DocumentType>().FirstOrDefaultAsync(e => e.Name == name && e.IsActive, ct);

    public Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken ct = default)
        => _db.Set<DocumentType>()
              .AnyAsync(e => e.Name == name && (excludeId == null || e.Id != excludeId), ct);

    public void Add(DocumentType entity)    => _db.Set<DocumentType>().Add(entity);
    public void Update(DocumentType entity) => _db.Set<DocumentType>().Update(entity);
    public void Remove(DocumentType entity) => _db.Set<DocumentType>().Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
