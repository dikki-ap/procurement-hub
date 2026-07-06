using ProcureHub.Modules.Procurement.Application.DTOs;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetPOById;

public record GetPOByIdQuery(Guid Id) : IQuery<PODto>;
