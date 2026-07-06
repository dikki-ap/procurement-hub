using ProcureHub.Modules.Procurement.Application.DTOs;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetRFQById;

public record GetRFQByIdQuery(Guid Id) : IQuery<RFQDto>;
