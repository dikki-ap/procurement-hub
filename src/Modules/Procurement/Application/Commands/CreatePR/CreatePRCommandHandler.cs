using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.Abstractions;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Commands.CreatePR;

public class CreatePRCommandHandler : ICommandHandler<CreatePRCommand, Guid>
{
    private readonly IPurchaseRequisitionRepository _repo;
    private readonly ICurrentUserService            _currentUser;
    private readonly ICacheService                  _cache;

    public CreatePRCommandHandler(
        IPurchaseRequisitionRepository repo,
        ICurrentUserService currentUser,
        ICacheService cache)
    {
        _repo        = repo;
        _currentUser = currentUser;
        _cache       = cache;
    }

    public async Task<Guid> Handle(CreatePRCommand command, CancellationToken ct)
    {
        var prNumber = await _repo.GenerateNextNumberAsync(command.CompanyId, ct);

        var pr = new PurchaseRequisition
        {
            CompanyId        = command.CompanyId,
            PRNumber         = prNumber,
            Title            = command.Title,
            Description      = command.Description,
            Department       = command.Department,
            DeliveryLocation = command.DeliveryLocation,
            RequiredDate     = command.RequiredDate,
            Notes            = command.Notes,
            RequestedById    = _currentUser.UserId ?? Guid.Empty,
        };

        foreach (var item in command.Items)
        {
            pr.Items.Add(new PRItem
            {
                MaterialId         = item.MaterialId,
                ItemDescription    = item.ItemDescription,
                Quantity           = item.Quantity,
                UnitOfMeasureId    = item.UnitOfMeasureId,
                UnitLabel          = item.UnitLabel,
                EstimatedUnitPrice = item.EstimatedUnitPrice,
                Notes              = item.Notes,
            });
        }

        pr.RecalculateTotalValue();

        _repo.Add(pr);
        await _repo.SaveChangesAsync(ct);

        _cache.RemoveByPrefix(CacheKeys.PurchaseRequisitions.Prefix);

        return pr.Id;
    }
}
