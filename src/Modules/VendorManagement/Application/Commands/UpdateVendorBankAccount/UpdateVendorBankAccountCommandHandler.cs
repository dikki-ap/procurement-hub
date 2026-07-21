using MediatR;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.VendorManagement.Domain.Entities;
using ProcureHub.Modules.VendorManagement.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Database;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.VendorManagement.Application.Commands.UpdateVendorBankAccount;

public class UpdateVendorBankAccountCommandHandler : ICommandHandler<UpdateVendorBankAccountCommand>
{
    private readonly IVendorBankAccountRepository _bankRepo;
    private readonly ICacheService                _cache;
    private readonly ApplicationDbContext         _db;

    public UpdateVendorBankAccountCommandHandler(
        IVendorBankAccountRepository bankRepo,
        ICacheService                cache,
        ApplicationDbContext         db)
    {
        _bankRepo = bankRepo;
        _cache    = cache;
        _db       = db;
    }

    public async Task<Unit> Handle(UpdateVendorBankAccountCommand command, CancellationToken ct)
    {
        var bankAccount = await _bankRepo.GetByIdAsync(command.BankAccountId, ct)
            ?? throw new NotFoundException("VendorBankAccount", command.BankAccountId);

        if (bankAccount.VendorId != command.VendorId)
            throw new ForbiddenException("Bank account does not belong to this vendor.");

        if (command.IsDefault && !bankAccount.IsDefault)
            await ClearDefaultFlagAsync(command.VendorId, command.BankAccountId, ct);

        bankAccount.BankName      = command.BankName;
        bankAccount.AccountNumber = command.AccountNumber;
        bankAccount.AccountName   = command.AccountName;
        bankAccount.BranchName    = command.BranchName;
        bankAccount.Currency      = command.Currency;
        bankAccount.IsDefault     = command.IsDefault;
        bankAccount.Notes         = command.Notes;

        _bankRepo.Update(bankAccount);
        await _bankRepo.SaveChangesAsync(ct);

        _cache.RemoveByPrefix(CacheKeys.Vendors.Prefix);

        return Unit.Value;
    }

    private async Task ClearDefaultFlagAsync(Guid vendorId, Guid excludeId, CancellationToken ct)
        => await _db.Set<VendorBankAccount>()
                    .Where(b => b.VendorId == vendorId && b.IsDefault && b.Id != excludeId)
                    .ExecuteUpdateAsync(s => s.SetProperty(b => b.IsDefault, false), ct);
}
