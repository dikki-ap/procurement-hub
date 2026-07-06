using ProcureHub.Modules.Procurement.Application.DTOs;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetBidComparison;

public record GetBidComparisonQuery(Guid RFQId) : IQuery<BidComparisonDto>;
