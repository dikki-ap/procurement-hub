using ProcureHub.Modules.Procurement.Application.DTOs;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetPRById;

public record GetPRByIdQuery(Guid Id) : IQuery<PRDto>;
