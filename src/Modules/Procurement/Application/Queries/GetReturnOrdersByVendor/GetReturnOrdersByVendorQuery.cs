using ProcureHub.Modules.Procurement.Application.DTOs;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetReturnOrdersByVendor;

public record GetReturnOrdersByVendorQuery(Guid VendorId) : IQuery<List<ReturnOrderListDto>>;
