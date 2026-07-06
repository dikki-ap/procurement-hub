using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProcureHub.Modules.ApprovalEngine.Domain.Repositories;
using ProcureHub.SharedKernel.Notifications;
using ProcureHub.SharedKernel.Abstractions;
using ProcureHub.SharedKernel.Database;
using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.ApprovalEngine.Infrastructure.Jobs;

public class ApprovalEscalationJob
{
    private readonly IApprovalWorkflowRepository _workflowRepo;
    private readonly ApplicationDbContext        _db;
    private readonly IEmailService               _email;
    private readonly INotificationService        _notification;
    private readonly ILogger<ApprovalEscalationJob> _logger;

    public ApprovalEscalationJob(
        IApprovalWorkflowRepository workflowRepo,
        ApplicationDbContext        db,
        IEmailService               email,
        INotificationService        notification,
        ILogger<ApprovalEscalationJob> logger)
    {
        _workflowRepo = workflowRepo;
        _db           = db;
        _email        = email;
        _notification = notification;
        _logger       = logger;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddHours(-2);
        var stale  = await _workflowRepo.GetPendingEscalationsAsync(cutoff, ct);

        foreach (var workflow in stale)
        {
            _logger.LogWarning(
                "Approval escalation — WorkflowId: {WorkflowId}, Ref: {ReferenceNumber}, Level: {Level}",
                workflow.Id, workflow.ReferenceNumber, workflow.CurrentLevel);

            var currentAssignments = workflow.Assignments
                .Where(a => a.Level == workflow.CurrentLevel)
                .ToList();

            var pendingHours = (int)(DateTime.UtcNow - workflow.CreatedAt).TotalHours;

            foreach (var assignment in currentAssignments)
            {
                var user = await _db.Set<User>()
                    .FirstOrDefaultAsync(u => u.Id == assignment.AssignedUserId, ct);
                if (user is null) continue;

                var html = EmailTemplates.ApprovalEscalation(
                    user.FullName, workflow.ReferenceNumber, pendingHours);

                await _email.SendAsync(user.Email,
                    $"Escalation Notice — {workflow.ReferenceNumber}", html, ct);

                await _notification.SendAsync(
                    user.Id,
                    "Pending Approval Escalation",
                    $"{workflow.ReferenceNumber} has been pending for {pendingHours} hours.",
                    link: $"/app/approvals/{workflow.Id}",
                    ct: ct);
            }

            workflow.LastEscalationSentAt = DateTime.UtcNow;
            _workflowRepo.Update(workflow);
            await _workflowRepo.SaveChangesAsync(ct);
        }

        _logger.LogInformation(
            "ApprovalEscalationJob completed. Stale workflows: {Count}", stale.Count);
    }
}
