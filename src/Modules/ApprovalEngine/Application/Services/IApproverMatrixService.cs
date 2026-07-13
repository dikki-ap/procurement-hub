using ProcureHub.Modules.ApprovalEngine.Application.Commands.SubmitForApproval;

namespace ProcureHub.Modules.ApprovalEngine.Application.Services;

public interface IApproverMatrixService
{
    Task<List<ApproverLevelRequest>> ResolveApproversAsync(
        Guid companyId, string referenceType, int requiredLevels, CancellationToken ct = default);
}
