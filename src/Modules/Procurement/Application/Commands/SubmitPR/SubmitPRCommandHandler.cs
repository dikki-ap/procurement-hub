using MediatR;
using ProcureHub.Modules.ApprovalEngine.Application.Commands.SubmitForApproval;
using ProcureHub.Modules.ApprovalEngine.Application.Services;
using ProcureHub.Modules.ApprovalEngine.Domain.Repositories;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Application.Commands.SubmitPR;

public class SubmitPRCommandHandler : ICommandHandler<SubmitPRCommand>
{
    private readonly IPurchaseRequisitionRepository _repo;
    private readonly ICacheService                  _cache;
    private readonly IMediator                      _mediator;
    private readonly IApprovalPolicyRepository      _policyRepo;
    private readonly IApproverMatrixService         _matrixService;

    public SubmitPRCommandHandler(
        IPurchaseRequisitionRepository repo,
        ICacheService                  cache,
        IMediator                      mediator,
        IApprovalPolicyRepository      policyRepo,
        IApproverMatrixService         matrixService)
    {
        _repo          = repo;
        _cache         = cache;
        _mediator      = mediator;
        _policyRepo    = policyRepo;
        _matrixService = matrixService;
    }

    public async Task<Unit> Handle(SubmitPRCommand command, CancellationToken ct)
    {
        var pr = await _repo.GetByIdWithItemsAsync(command.Id, ct)
            ?? throw new NotFoundException("PurchaseRequisition", command.Id);

        var policy = await _policyRepo.FindMatchingAsync(pr.CompanyId, "PR", pr.TotalEstimatedValue, ct);
        var requiredLevels = policy?.RequiredLevels ?? 1;

        var approvers = await _matrixService.ResolveApproversAsync(pr.CompanyId, "PR", requiredLevels, ct);

        await _mediator.Send(new SubmitForApprovalCommand(
            pr.CompanyId,
            "PR",
            pr.Id,
            pr.PRNumber,
            pr.Title,
            pr.TotalEstimatedValue,
            false,
            false,
            pr.RequestedById,
            approvers), ct);

        pr.Submit();

        _repo.Update(pr);
        await _repo.SaveChangesAsync(ct);

        _cache.RemoveByPrefix(CacheKeys.PurchaseRequisitions.Prefix);
        return Unit.Value;
    }
}
