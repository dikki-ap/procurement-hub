using ProcureHub.Modules.Procurement.Application.DTOs;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetGRNById;

public record GetGRNByIdQuery(Guid Id) : IQuery<GRNDto>;
