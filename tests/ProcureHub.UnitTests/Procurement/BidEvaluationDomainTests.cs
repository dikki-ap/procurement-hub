using FluentAssertions;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.Modules.Procurement.Domain.Enums;
using ProcureHub.Modules.Procurement.Domain.Events;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.UnitTests.Procurement;

public class BidEvaluationDomainTests
{
    private static readonly Guid _rfqId         = Guid.NewGuid();
    private static readonly Guid _quotationId    = Guid.NewGuid();
    private static readonly Guid _vendorId       = Guid.NewGuid();

    // ── Create: weights not summing to 100 ───────────────────────────────────

    [Fact]
    public void Create_WhenWeightsDoNotSumTo100_ShouldThrowBusinessRuleException()
    {
        var act = () => BidEvaluation.Create(_rfqId, 40m, 30m, 20m); // sum = 90

        act.Should().Throw<BusinessRuleException>()
           .WithMessage("*Weights must sum to 100*");
    }

    [Fact]
    public void Create_WhenWeightsSumTo100_ShouldSucceed()
    {
        var eval = BidEvaluation.Create(_rfqId, 50m, 30m, 20m);

        eval.Should().NotBeNull();
        eval.PriceWeight.Should().Be(50m);
    }

    [Fact]
    public void Create_WithFractionalWeightsSummingTo100_ShouldSucceed()
    {
        // 33.33 + 33.33 + 33.34 = 100.00 (within tolerance)
        var act = () => BidEvaluation.Create(_rfqId, 33.33m, 33.33m, 33.34m);

        act.Should().NotThrow();
    }

    // ── Weighted score calculation ────────────────────────────────────────────

    [Fact]
    public void CalculateWeightedScore_ShouldReturnCorrectResult()
    {
        // weights: price=50, quality=30, delivery=20
        // scores:  price=80, quality=90, delivery=100
        // expected: (80*50 + 90*30 + 100*20) / 100 = (4000 + 2700 + 2000) / 100 = 87
        var eval = BidEvaluation.Create(_rfqId, 50m, 30m, 20m);

        var score = eval.CalculateWeightedScore(80m, 90m, 100m);

        score.Should().Be(87.00m);
    }

    [Fact]
    public void CalculateWeightedScore_WithEqualWeights_ShouldBeAverageOfScores()
    {
        // weights: 33.33 + 33.33 + 33.34 ≈ 100
        var eval = BidEvaluation.Create(_rfqId, 33.33m, 33.33m, 33.34m);

        var score = eval.CalculateWeightedScore(80m, 80m, 80m);

        score.Should().BeApproximately(80m, 0.1m);
    }

    // ── Award: raises BidAwardedEvent ────────────────────────────────────────

    [Fact]
    public void Award_ShouldChangeStatusToAwarded()
    {
        var eval = BidEvaluation.Create(_rfqId, 50m, 30m, 20m);

        eval.Award(_quotationId, _vendorId);

        eval.Status.Should().Be(EvaluationStatus.Awarded);
        eval.AwardedQuotationId.Should().Be(_quotationId);
        eval.AwardedVendorId.Should().Be(_vendorId);
    }

    [Fact]
    public void Award_ShouldRaiseBidAwardedEvent()
    {
        var eval = BidEvaluation.Create(_rfqId, 50m, 30m, 20m);

        eval.Award(_quotationId, _vendorId);

        var evt = eval.DomainEvents.OfType<BidAwardedEvent>().Single();
        evt.RFQId.Should().Be(_rfqId);
        evt.AwardedQuotationId.Should().Be(_quotationId);
        evt.AwardedVendorId.Should().Be(_vendorId);
    }

    [Fact]
    public void Award_WhenAlreadyAwarded_ShouldThrowBusinessRuleException()
    {
        var eval = BidEvaluation.Create(_rfqId, 50m, 30m, 20m);
        eval.Award(_quotationId, _vendorId);

        var act = () => eval.Award(Guid.NewGuid(), Guid.NewGuid());

        act.Should().Throw<BusinessRuleException>()
           .WithMessage("*already been awarded*");
    }
}
