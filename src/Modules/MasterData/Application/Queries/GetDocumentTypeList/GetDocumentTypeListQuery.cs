using ProcureHub.Modules.MasterData.Application.DTOs;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Queries.GetDocumentTypeList;

public record GetDocumentTypeListQuery : IQuery<List<DocumentTypeDto>>;
