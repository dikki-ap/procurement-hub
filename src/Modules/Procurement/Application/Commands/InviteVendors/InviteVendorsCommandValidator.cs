using FluentValidation;

namespace ProcureHub.Modules.Procurement.Application.Commands.InviteVendors;

public class InviteVendorsCommandValidator : AbstractValidator<InviteVendorsCommand>
{
    public InviteVendorsCommandValidator()
    {
        RuleFor(x => x.RFQId).NotEmpty();
        RuleFor(x => x.VendorIds).NotEmpty().WithMessage("At least one vendor must be specified.");
        RuleForEach(x => x.VendorIds).NotEmpty();
    }
}
