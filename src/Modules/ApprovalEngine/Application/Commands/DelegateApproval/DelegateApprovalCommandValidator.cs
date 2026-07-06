using FluentValidation;

namespace ProcureHub.Modules.ApprovalEngine.Application.Commands.DelegateApproval;

public class DelegateApprovalCommandValidator : AbstractValidator<DelegateApprovalCommand>
{
    public DelegateApprovalCommandValidator()
    {
        RuleFor(x => x.WorkflowId).NotEmpty();
        RuleFor(x => x.ApproverId).NotEmpty();
        RuleFor(x => x.DelegateToUserId).NotEmpty();
        RuleFor(x => x.DelegateToUserName).NotEmpty();
    }
}
