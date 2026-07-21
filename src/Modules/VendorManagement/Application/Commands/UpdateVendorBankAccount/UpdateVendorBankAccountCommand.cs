using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.VendorManagement.Application.Commands.UpdateVendorBankAccount;

public record UpdateVendorBankAccountCommand(
    Guid    BankAccountId,
    Guid    VendorId,
    string  BankName,
    string  AccountNumber,
    string  AccountName,
    string? BranchName,
    string  Currency,
    bool    IsDefault,
    string? Notes) : ICommand;
