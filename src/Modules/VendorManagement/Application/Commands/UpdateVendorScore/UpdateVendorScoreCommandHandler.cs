using MediatR;
using ProcureHub.Modules.VendorManagement.Domain.Entities;
using ProcureHub.Modules.VendorManagement.Domain.Enums;
using ProcureHub.Modules.VendorManagement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.VendorManagement.Application.Commands.UpdateVendorScore;

public class UpdateVendorScoreCommandHandler : ICommandHandler<UpdateVendorScoreCommand>
{
    private readonly IVendorScoreRepository _scoreRepo;

    public UpdateVendorScoreCommandHandler(IVendorScoreRepository scoreRepo)
        => _scoreRepo = scoreRepo;

    public async Task<Unit> Handle(UpdateVendorScoreCommand command, CancellationToken ct)
    {
        var now     = DateTime.UtcNow;
        var current = await _scoreRepo.GetCurrentAsync(command.VendorId, ct);

        // Simple scoring: each factor contributes 0–100 points
        var deliveryScore = command.DeliveredOnTime   ? 100m : 60m;
        var qualityScore  = command.HasQualityIssues  ? 50m  : 100m;
        var priceScore    = 80m;  // static; updated separately when bid awarded
        var docScore      = 80m;  // static; updated by document expiry job

        var totalScore    = (deliveryScore + qualityScore + priceScore + docScore) / 4m;

        var tier = totalScore switch
        {
            >= 90 => VendorTier.Gold,
            >= 75 => VendorTier.Silver,
            >= 60 => VendorTier.Bronze,
            _     => VendorTier.Probation,
        };

        if (current != null && current.PeriodYear == now.Year && current.PeriodQuarter == ((now.Month - 1) / 3) + 1)
        {
            // Update current quarter score (running average)
            current.DeliveryScore = (current.DeliveryScore + deliveryScore) / 2m;
            current.QualityScore  = (current.QualityScore  + qualityScore)  / 2m;
            current.TotalScore    = (current.DeliveryScore + current.QualityScore + (current.PriceScore ?? 80) + (current.DocScore ?? 80)) / 4m;
            current.Tier          = tier;
            current.CalculatedAt  = now;
            _scoreRepo.Update(current);
        }
        else
        {
            var score = new VendorScore
            {
                VendorId      = command.VendorId,
                PeriodYear    = now.Year,
                PeriodQuarter = ((now.Month - 1) / 3) + 1,
                DeliveryScore = deliveryScore,
                QualityScore  = qualityScore,
                PriceScore    = priceScore,
                DocScore      = docScore,
                TotalScore    = totalScore,
                Tier          = tier,
                CalculatedAt  = now,
                CreatedAt     = now,
            };
            _scoreRepo.Add(score);
        }

        await _scoreRepo.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
