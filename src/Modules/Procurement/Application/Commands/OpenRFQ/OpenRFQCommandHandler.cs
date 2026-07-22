using MediatR;
using ProcureHub.Modules.ApprovalEngine.Application.Commands.SubmitForApproval;
using ProcureHub.Modules.ApprovalEngine.Application.Services;
using ProcureHub.Modules.ApprovalEngine.Domain.Repositories;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Application.Commands.OpenRFQ;

public class OpenRFQCommandHandler : ICommandHandler<OpenRFQCommand>
{
    private readonly IRFQRepository           _repo;
    private readonly ICacheService            _cache;
    private readonly IMediator                _mediator;
    private readonly IApprovalPolicyRepository _policyRepo;
    private readonly IApproverMatrixService   _matrixService;

    public OpenRFQCommandHandler(
        IRFQRepository            repo,
        ICacheService             cache,
        IMediator                 mediator,
        IApprovalPolicyRepository policyRepo,
        IApproverMatrixService    matrixService)
    {
        _repo          = repo;
        _cache         = cache;
        _mediator      = mediator;
        _policyRepo    = policyRepo;
        _matrixService = matrixService;
    }

    public async Task<Unit> Handle(OpenRFQCommand command, CancellationToken ct)
    {
        var rfq = await _repo.GetByIdWithDetailsAsync(command.Id, ct)
            ?? throw new NotFoundException("RFQ", command.Id);

        var policy = await _policyRepo.FindMatchingAsync(rfq.CompanyId, "RFQ", 0, ct);

        if (policy is not null)
        {
            // Approval required — submit to approval engine, set PendingApproval status
            var approvers = await _matrixService.ResolveApproversAsync(
                rfq.CompanyId, "RFQ", policy.RequiredLevels, ct);

            rfq.SubmitForApproval();

            await _mediator.Send(new SubmitForApprovalCommand(
                rfq.CompanyId,
                "RFQ",
                rfq.Id,
                rfq.RFQNumber,
                rfq.Title,
                0,
                false,
                false,
                rfq.CreatedById ?? Guid.Empty,
                approvers), ct);
        }
        else
        {
            // No approval policy — open immediately
            rfq.Open();
        }

        _repo.Update(rfq);
        await _repo.SaveChangesAsync(ct);

        _cache.RemoveByPrefix(CacheKeys.RFQs.Prefix);
        return Unit.Value;
    }
}
