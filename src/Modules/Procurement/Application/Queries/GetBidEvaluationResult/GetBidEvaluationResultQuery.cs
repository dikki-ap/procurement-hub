using ProcureHub.Modules.Procurement.Application.DTOs;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetBidEvaluationResult;

public record GetBidEvaluationResultQuery(Guid RFQId) : IQuery<BidEvaluationDto?>;
