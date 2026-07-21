using MediatR;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Application.Commands.WithdrawQuotation;

public class WithdrawQuotationCommandHandler : ICommandHandler<WithdrawQuotationCommand>
{
    private readonly IVendorQuotationRepository _repo;

    public WithdrawQuotationCommandHandler(IVendorQuotationRepository repo) => _repo = repo;

    public async Task<Unit> Handle(WithdrawQuotationCommand command, CancellationToken ct)
    {
        var quotation = await _repo.GetByIdWithItemsAsync(command.QuotationId, ct)
                        ?? throw new NotFoundException("VendorQuotation", command.QuotationId);

        if (quotation.VendorId != command.RequestingVendorId)
            throw new ForbiddenException("You are not authorised to withdraw this quotation.");

        quotation.Withdraw();
        _repo.Update(quotation);
        await _repo.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
