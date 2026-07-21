using MediatR;
using ProcureHub.Modules.VendorManagement.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.VendorManagement.Application.Commands.DeleteVendorBankAccount;

public class DeleteVendorBankAccountCommandHandler : ICommandHandler<DeleteVendorBankAccountCommand>
{
    private readonly IVendorBankAccountRepository _bankRepo;
    private readonly ICacheService                _cache;

    public DeleteVendorBankAccountCommandHandler(
        IVendorBankAccountRepository bankRepo,
        ICacheService                cache)
    {
        _bankRepo = bankRepo;
        _cache    = cache;
    }

    public async Task<Unit> Handle(DeleteVendorBankAccountCommand command, CancellationToken ct)
    {
        var bankAccount = await _bankRepo.GetByIdAsync(command.BankAccountId, ct)
            ?? throw new NotFoundException("VendorBankAccount", command.BankAccountId);

        if (bankAccount.VendorId != command.VendorId)
            throw new ForbiddenException("Bank account does not belong to this vendor.");

        _bankRepo.Remove(bankAccount);
        await _bankRepo.SaveChangesAsync(ct);

        _cache.RemoveByPrefix(CacheKeys.Vendors.Prefix);

        return Unit.Value;
    }
}
