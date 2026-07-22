using MediatR;
using ProcureHub.Modules.VendorManagement.Domain.Entities;
using ProcureHub.Modules.VendorManagement.Domain.Enums;
using ProcureHub.Modules.VendorManagement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.VendorManagement.Application.Commands.UpdateVendorScore;

public class UpdateVendorScoreCommandHandler : ICommandHandler<UpdateVendorScoreCommand>
{
    private readonly IVendorScoreRepository _scoreRepo;
    private readonly IVendorRepository      _vendorRepo;

    public UpdateVendorScoreCommandHandler(
        IVendorScoreRepository scoreRepo,
        IVendorRepository      vendorRepo)
    {
        _scoreRepo  = scoreRepo;
        _vendorRepo = vendorRepo;
    }

    public async Task<Unit> Handle(UpdateVendorScoreCommand command, CancellationToken ct)
    {
        var now     = DateTime.UtcNow;
        var quarter = ((now.Month - 1) / 3) + 1;
        var current = await _scoreRepo.GetCurrentAsync(command.VendorId, ct);

        var deliveryScore = command.DeliveredOnTime  ? 100m : 60m;
        var qualityScore  = command.HasQualityIssues ? 50m  : 100m;
        var priceScore    = 80m;
        var docScore      = 80m;

        decimal finalTotal;

        if (current != null && current.PeriodYear == now.Year && current.PeriodQuarter == quarter)
        {
            // Running average within the same quarter
            current.DeliveryScore = ((current.DeliveryScore ?? deliveryScore) + deliveryScore) / 2m;
            current.QualityScore  = ((current.QualityScore  ?? qualityScore)  + qualityScore)  / 2m;
            current.PriceScore    ??= priceScore;
            current.DocScore      ??= docScore;
            current.TotalScore    = (current.DeliveryScore + current.QualityScore + current.PriceScore + current.DocScore) / 4m;
            current.Tier          = ToTier(current.TotalScore.Value);
            current.CalculatedAt  = now;
            _scoreRepo.Update(current);
            finalTotal = current.TotalScore.Value;
        }
        else
        {
            var totalScore = (deliveryScore + qualityScore + priceScore + docScore) / 4m;
            var score = new VendorScore
            {
                VendorId      = command.VendorId,
                PeriodYear    = now.Year,
                PeriodQuarter = quarter,
                DeliveryScore = deliveryScore,
                QualityScore  = qualityScore,
                PriceScore    = priceScore,
                DocScore      = docScore,
                TotalScore    = totalScore,
                Tier          = ToTier(totalScore),
                CalculatedAt  = now,
                CreatedAt     = now,
            };
            _scoreRepo.Add(score);
            finalTotal = totalScore;
        }

        await _scoreRepo.SaveChangesAsync(ct);

        // Sync denormalized Score and Tier back onto the Vendor aggregate
        var vendor = await _vendorRepo.GetByIdAsync(command.VendorId, ct);
        if (vendor is not null)
        {
            vendor.Score = finalTotal;
            vendor.Tier  = ToTier(finalTotal);
            _vendorRepo.Update(vendor);
            await _vendorRepo.SaveChangesAsync(ct);
        }

        return Unit.Value;
    }

    private static VendorTier ToTier(decimal total) => total switch
    {
        >= 90 => VendorTier.Gold,
        >= 75 => VendorTier.Silver,
        >= 60 => VendorTier.Bronze,
        _     => VendorTier.Probation,
    };
}
