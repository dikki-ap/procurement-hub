using ProcureHub.Modules.MasterData.Application.DTOs;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Queries.GetDocumentTypeList;

public class GetDocumentTypeListQueryHandler : IQueryHandler<GetDocumentTypeListQuery, List<DocumentTypeDto>>
{
    private readonly IDocumentTypeRepository _repo;
    private readonly ICacheService           _cache;

    public GetDocumentTypeListQueryHandler(IDocumentTypeRepository repo, ICacheService cache)
    {
        _repo  = repo;
        _cache = cache;
    }

    public Task<List<DocumentTypeDto>> Handle(GetDocumentTypeListQuery query, CancellationToken ct)
        => _cache.GetOrSetAsync(
            CacheKeys.DocumentTypes.List,
            async () =>
            {
                var list = await _repo.GetAllAsync(ct);
                return list.Select(e => new DocumentTypeDto(e.Id, e.Name, e.IsActive, e.AllowedExtensions, e.MaxFileSizeMb)).ToList();
            },
            CacheTTL.DocumentTypes);
}
