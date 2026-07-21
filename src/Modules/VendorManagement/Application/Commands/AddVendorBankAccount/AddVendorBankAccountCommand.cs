using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.VendorManagement.Application.Commands.AddVendorBankAccount;

public record AddVendorBankAccountCommand(
    Guid    VendorId,
    string  BankName,
    string  AccountNumber,
    string  AccountName,
    string? BranchName,
    string  Currency,
    bool    IsDefault,
    string? Notes) : ICommand<Guid>;
