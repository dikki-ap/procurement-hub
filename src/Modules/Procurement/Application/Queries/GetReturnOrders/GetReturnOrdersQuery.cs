using ProcureHub.Modules.Procurement.Application.DTOs;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetReturnOrders;

public record GetReturnOrdersQuery(Guid CompanyId) : IQuery<List<ReturnOrderListDto>>;
