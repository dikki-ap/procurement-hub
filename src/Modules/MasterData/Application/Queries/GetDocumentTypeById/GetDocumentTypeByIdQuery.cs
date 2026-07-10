using ProcureHub.Modules.MasterData.Application.DTOs;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Queries.GetDocumentTypeById;

public record GetDocumentTypeByIdQuery(Guid Id) : IQuery<DocumentTypeDto>;
