using FluentAssertions;
using ProcureHub.Modules.ApprovalEngine.Domain.Entities;
using ProcureHub.Modules.ApprovalEngine.Domain.Enums;
using ProcureHub.Modules.ApprovalEngine.Domain.Events;
using ProcureHub.Modules.ApprovalEngine.Domain.Services;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.UnitTests.ApprovalEngine;

public class ApprovalStateMachineTests
{
    private static readonly Guid _approverId  = Guid.NewGuid();
    private static readonly Guid _approver2Id = Guid.NewGuid();
    private const string ApproverName = "Alice Approver";

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static ApprovalWorkflow CreateWorkflow(int maxLevel = 2, int currentLevel = 1)
    {
        var w = ApprovalWorkflow.Create(
            Guid.NewGuid(), "PR", Guid.NewGuid(), "PR-0001", "Test PR",
            100_000m, false, false, Guid.NewGuid(), maxLevel);
        w.CurrentLevel = currentLevel;
        w.ClearDomainEvents();

        w.Assignments.Add(new ApproverAssignment
        {
            WorkflowId       = w.Id,
            Level            = 1,
            AssignedUserId   = _approverId,
            AssignedUserName = ApproverName,
            IsDelegate       = false,
        });
        w.Assignments.Add(new ApproverAssignment
        {
            WorkflowId       = w.Id,
            Level            = 2,
            AssignedUserId   = _approver2Id,
            AssignedUserName = "Bob Approver",
            IsDelegate       = false,
        });
        return w;
    }

    // Approve/Revise/Reject/Delegate don't query the DB, so null context is safe here.
    private static ApprovalStateMachine CreateStateMachine() => new(null!);

    // ── 4. Last level approve → Approved ─────────────────────────────────────

    [Fact]
    public void Approve_LastLevel_SetsWorkflowStatusToApproved()
    {
        var w  = CreateWorkflow(maxLevel: 2, currentLevel: 2);
        var sm = CreateStateMachine();

        sm.Approve(w, _approver2Id, "Bob Approver");

        w.Status.Should().Be(WorkflowStatus.Approved);
        w.CompletedAt.Should().NotBeNull();
        w.DomainEvents.Should().ContainSingle(e => e is ApprovalCompletedEvent);
    }

    [Fact]
    public void Approve_NotLastLevel_AdvancesCurrentLevel()
    {
        var w  = CreateWorkflow(maxLevel: 2, currentLevel: 1);
        var sm = CreateStateMachine();

        sm.Approve(w, _approverId, ApproverName);

        w.CurrentLevel.Should().Be(2);
        w.Status.Should().Be(WorkflowStatus.Pending);
    }

    // ── 5. Not current approver → ForbiddenException ─────────────────────────

    [Fact]
    public void Approve_NotCurrentApprover_ThrowsForbiddenException()
    {
        var w          = CreateWorkflow(maxLevel: 2, currentLevel: 1);
        var sm         = CreateStateMachine();
        var strangerIds = Guid.NewGuid();

        var act = () => sm.Approve(w, strangerIds, "Stranger");

        act.Should().Throw<ForbiddenException>();
    }

    // ── 6. Revise at level 1 → status Revised ────────────────────────────────

    [Fact]
    public void Revise_AtLevel1_SetsStatusToRevised()
    {
        var w      = CreateWorkflow(maxLevel: 2, currentLevel: 1);
        var sm     = CreateStateMachine();
        var reason = "This document needs more detail in section 3 of the requirements.";

        sm.Revise(w, _approverId, ApproverName, reason);

        w.Status.Should().Be(WorkflowStatus.Revised);
        w.DomainEvents.Should().ContainSingle(e => e is ApprovalRevisedEvent);
    }

    [Fact]
    public void Revise_AtLevel2_DecrementsCurrentLevel()
    {
        var w      = CreateWorkflow(maxLevel: 2, currentLevel: 2);
        var sm     = CreateStateMachine();
        var reason = "The vendor selection criteria need to be revisited before proceeding.";

        sm.Revise(w, _approver2Id, "Bob Approver", reason);

        w.CurrentLevel.Should().Be(1);
        w.Status.Should().Be(WorkflowStatus.Pending);
    }

    // ── 7. Revise without reason → BusinessRuleException ─────────────────────

    [Fact]
    public void Revise_WithoutReason_ThrowsBusinessRuleException()
    {
        var w  = CreateWorkflow();
        var sm = CreateStateMachine();

        var act = () => sm.Revise(w, _approverId, ApproverName, "Short");

        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*20 characters*");
    }

    // ── 8. Reject without reason → BusinessRuleException ─────────────────────

    [Fact]
    public void Reject_WithoutReason_ThrowsBusinessRuleException()
    {
        var w  = CreateWorkflow();
        var sm = CreateStateMachine();

        var act = () => sm.Reject(w, _approverId, ApproverName, "Too short");

        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*20 characters*");
    }

    // ── 9. Reject increments iteration ───────────────────────────────────────

    [Fact]
    public void Reject_WithReason_IncrementsIterationAndSetsStatusRejected()
    {
        var w      = CreateWorkflow();
        var sm     = CreateStateMachine();
        var reason = "The total price exceeds the approved budget by more than 15%.";

        sm.Reject(w, _approverId, ApproverName, reason);

        w.Status.Should().Be(WorkflowStatus.Rejected);
        w.Iteration.Should().Be(2); // started at 1
        w.DomainEvents.Should().ContainSingle(e => e is ApprovalRejectedEvent);
    }

    // ── 10. Delegate to self → BusinessRuleException ──────────────────────────

    [Fact]
    public void Delegate_ToSelf_ThrowsBusinessRuleException()
    {
        var w  = CreateWorkflow();
        var sm = CreateStateMachine();

        var act = () => sm.Delegate(w, _approverId, ApproverName, _approverId, ApproverName);

        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*delegate to yourself*");
    }

    // ── Not-pending state guard ───────────────────────────────────────────────

    [Fact]
    public void Approve_OnNonPendingWorkflow_ThrowsBusinessRuleException()
    {
        var w  = CreateWorkflow();
        w.MarkApproved();
        w.ClearDomainEvents();
        var sm = CreateStateMachine();

        var act = () => sm.Approve(w, _approverId, ApproverName);

        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*pending state*");
    }
}
