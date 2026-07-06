using Hangfire;
using Microsoft.Extensions.Logging;
using ProcureHub.Modules.ApprovalEngine.Domain.Repositories;

namespace ProcureHub.Modules.ApprovalEngine.Infrastructure.Jobs;

public class ApprovalEscalationJob
{
    private readonly IApprovalWorkflowRepository _workflowRepo;
    private readonly ILogger<ApprovalEscalationJob> _logger;

    public ApprovalEscalationJob(
        IApprovalWorkflowRepository workflowRepo,
        ILogger<ApprovalEscalationJob> logger)
    {
        _workflowRepo = workflowRepo;
        _logger       = logger;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        // Flag workflows pending for more than 2 hours for escalation
        var cutoff   = DateTime.UtcNow.AddHours(-2);
        var stale    = await _workflowRepo.GetPendingEscalationsAsync(cutoff, ct);

        foreach (var workflow in stale)
        {
            _logger.LogWarning(
                "Approval escalation — WorkflowId: {WorkflowId}, Ref: {ReferenceNumber}, Level: {Level}, PendingSince: {Since}",
                workflow.Id, workflow.ReferenceNumber, workflow.CurrentLevel, workflow.CreatedAt);

            // TODO: Send escalation notification to approver and their manager via IEmailService.
        }

        _logger.LogInformation(
            "ApprovalEscalationJob completed. Stale workflows: {Count}", stale.Count);
    }
}
