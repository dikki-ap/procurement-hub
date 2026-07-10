using ProcureHub.Modules.MasterData.Application.DTOs;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.MasterData.Application.Queries.GetDocumentTypeById;

public class GetDocumentTypeByIdQueryHandler : IQueryHandler<GetDocumentTypeByIdQuery, DocumentTypeDto>
{
    private readonly IDocumentTypeRepository _repo;

    public GetDocumentTypeByIdQueryHandler(IDocumentTypeRepository repo) => _repo = repo;

    public async Task<DocumentTypeDto> Handle(GetDocumentTypeByIdQuery query, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(query.Id, ct)
            ?? throw new NotFoundException("DocumentType", query.Id);

        return new DocumentTypeDto(entity.Id, entity.Name, entity.IsActive, entity.AllowedExtensions, entity.MaxFileSizeMb);
    }
}
