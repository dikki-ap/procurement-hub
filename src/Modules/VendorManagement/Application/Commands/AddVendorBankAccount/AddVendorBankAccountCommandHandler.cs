using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.VendorManagement.Domain.Entities;
using ProcureHub.Modules.VendorManagement.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Database;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.VendorManagement.Application.Commands.AddVendorBankAccount;

public class AddVendorBankAccountCommandHandler : ICommandHandler<AddVendorBankAccountCommand, Guid>
{
    private readonly IVendorRepository            _vendorRepo;
    private readonly IVendorBankAccountRepository _bankRepo;
    private readonly ICacheService                _cache;
    private readonly ApplicationDbContext         _db;

    public AddVendorBankAccountCommandHandler(
        IVendorRepository            vendorRepo,
        IVendorBankAccountRepository bankRepo,
        ICacheService                cache,
        ApplicationDbContext         db)
    {
        _vendorRepo = vendorRepo;
        _bankRepo   = bankRepo;
        _cache      = cache;
        _db         = db;
    }

    public async Task<Guid> Handle(AddVendorBankAccountCommand command, CancellationToken ct)
    {
        var vendor = await _vendorRepo.GetByIdAsync(command.VendorId, ct)
            ?? throw new NotFoundException("Vendor", command.VendorId);

        if (command.IsDefault)
            await ClearDefaultFlagAsync(vendor.Id, ct);

        var bankAccount = new VendorBankAccount
        {
            VendorId      = vendor.Id,
            BankName      = command.BankName,
            AccountNumber = command.AccountNumber,
            AccountName   = command.AccountName,
            BranchName    = command.BranchName,
            Currency      = command.Currency,
            IsDefault     = command.IsDefault,
            Notes         = command.Notes,
        };

        _bankRepo.Add(bankAccount);
        await _bankRepo.SaveChangesAsync(ct);

        _cache.RemoveByPrefix(CacheKeys.Vendors.Prefix);

        return bankAccount.Id;
    }

    private async Task ClearDefaultFlagAsync(Guid vendorId, CancellationToken ct)
        => await _db.Set<VendorBankAccount>()
                    .Where(b => b.VendorId == vendorId && b.IsDefault)
                    .ExecuteUpdateAsync(s => s.SetProperty(b => b.IsDefault, false), ct);
}
