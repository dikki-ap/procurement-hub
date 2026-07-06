using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Application.Commands.CreateRFQ;

public class CreateRFQCommandHandler : ICommandHandler<CreateRFQCommand, Guid>
{
    private readonly IRFQRepository _repo;
    private readonly IPurchaseRequisitionRepository _prRepo;
    private readonly ICacheService  _cache;

    public CreateRFQCommandHandler(
        IRFQRepository repo,
        IPurchaseRequisitionRepository prRepo,
        ICacheService cache)
    {
        _repo   = repo;
        _prRepo = prRepo;
        _cache  = cache;
    }

    public async Task<Guid> Handle(CreateRFQCommand command, CancellationToken ct)
    {
        if (command.PurchaseRequisitionId.HasValue)
        {
            var pr = await _prRepo.GetByIdAsync(command.PurchaseRequisitionId.Value, ct)
                ?? throw new NotFoundException("PurchaseRequisition", command.PurchaseRequisitionId.Value);
        }

        var rfqNumber = await _repo.GenerateNextNumberAsync(command.CompanyId, ct);

        var rfq = new RFQ
        {
            CompanyId             = command.CompanyId,
            RFQNumber             = rfqNumber,
            Title                 = command.Title,
            PurchaseRequisitionId = command.PurchaseRequisitionId,
            BidDeadline           = command.BidDeadline,
            DeliveryDate          = command.DeliveryDate,
            Notes                 = command.Notes,
            Terms                 = command.Terms,
        };

        foreach (var item in command.Items)
        {
            rfq.Items.Add(new RFQItem
            {
                PRItemId        = item.PRItemId,
                MaterialId      = item.MaterialId,
                ItemDescription = item.ItemDescription,
                Quantity        = item.Quantity,
                UnitOfMeasureId = item.UnitOfMeasureId,
                UnitLabel       = item.UnitLabel,
            });
        }

        _repo.Add(rfq);
        await _repo.SaveChangesAsync(ct);

        _cache.RemoveByPrefix(CacheKeys.RFQs.Prefix);

        return rfq.Id;
    }
}
