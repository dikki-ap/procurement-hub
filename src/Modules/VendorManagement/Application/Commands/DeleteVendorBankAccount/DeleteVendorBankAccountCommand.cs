using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.VendorManagement.Application.Commands.DeleteVendorBankAccount;

public record DeleteVendorBankAccountCommand(Guid BankAccountId, Guid VendorId) : ICommand;
