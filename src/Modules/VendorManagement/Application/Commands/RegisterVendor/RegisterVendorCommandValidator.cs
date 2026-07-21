using FluentValidation;

namespace ProcureHub.Modules.VendorManagement.Application.Commands.RegisterVendor;

public class RegisterVendorCommandValidator : AbstractValidator<RegisterVendorCommand>
{
    public RegisterVendorCommandValidator()
    {
        RuleFor(x => x.LegalName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.TradeName).MaximumLength(255).When(x => x.TradeName != null);
        RuleFor(x => x.VendorType).IsInEnum();
        RuleFor(x => x.Npwp).MaximumLength(30).When(x => x.Npwp != null);
        RuleFor(x => x.City).MaximumLength(100).When(x => x.City != null);
        RuleFor(x => x.Province).MaximumLength(100).When(x => x.Province != null);
        RuleFor(x => x.PostalCode).MaximumLength(10).When(x => x.PostalCode != null);
        RuleFor(x => x.Country).MaximumLength(100).When(x => x.Country != null);
        RuleFor(x => x.ContactName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.ContactEmail).NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(x => x.ContactPhone).MaximumLength(30).When(x => x.ContactPhone != null);
    }
}
