using MediatR;
using ProcureHub.Modules.Procurement.Domain.Enums;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Application.Commands.AwardVendor;

public class AwardVendorCommandHandler : ICommandHandler<AwardVendorCommand>
{
    private readonly IBidEvaluationRepository   _evalRepo;
    private readonly IVendorQuotationRepository _quotationRepo;

    public AwardVendorCommandHandler(
        IBidEvaluationRepository evalRepo,
        IVendorQuotationRepository quotationRepo)
    {
        _evalRepo      = evalRepo;
        _quotationRepo = quotationRepo;
    }

    public async Task<Unit> Handle(AwardVendorCommand command, CancellationToken ct)
    {
        var evaluation = await _evalRepo.GetByRFQIdAsync(command.RFQId, ct)
                         ?? throw new NotFoundException("BidEvaluation for RFQ", command.RFQId);

        evaluation.Award(command.QuotationId, command.VendorId);
        _evalRepo.Update(evaluation);

        // Update quotation statuses
        var allQuotations = await _quotationRepo.GetByRFQIdAsync(command.RFQId, ct);
        foreach (var q in allQuotations.Where(q => q.Status == QuotationStatus.Submitted))
        {
            if (q.Id == command.QuotationId)
                q.MarkAwarded();
            else
                q.MarkRejected();

            _quotationRepo.Update(q);
        }

        await _evalRepo.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
