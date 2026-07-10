using ProcureHub.Modules.MasterData.Domain.Entities;

namespace ProcureHub.Modules.MasterData.Domain.Repositories;

public interface IDocumentTypeRepository
{
    Task<List<DocumentType>> GetAllAsync(CancellationToken ct = default);
    Task<DocumentType?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<DocumentType?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken ct = default);
    void Add(DocumentType entity);
    void Update(DocumentType entity);
    void Remove(DocumentType entity);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
