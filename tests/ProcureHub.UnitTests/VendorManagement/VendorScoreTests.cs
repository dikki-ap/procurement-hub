using Moq;
using ProcureHub.Modules.VendorManagement.Application.Commands.UpdateVendorScore;
using ProcureHub.Modules.VendorManagement.Domain.Entities;
using ProcureHub.Modules.VendorManagement.Domain.Enums;
using ProcureHub.Modules.VendorManagement.Domain.Repositories;

namespace ProcureHub.UnitTests.VendorManagement;

public class VendorScoreTests
{
    private static (UpdateVendorScoreCommandHandler handler, Mock<IVendorScoreRepository> repoMock, List<VendorScore> stored)
        BuildHandler(VendorScore? existing = null)
    {
        var stored = new List<VendorScore>();
        var mock   = new Mock<IVendorScoreRepository>();

        mock.Setup(r => r.GetCurrentAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        mock.Setup(r => r.Add(It.IsAny<VendorScore>()))
            .Callback<VendorScore>(stored.Add);

        mock.Setup(r => r.Update(It.IsAny<VendorScore>()))
            .Callback<VendorScore>(s => { if (!stored.Contains(s)) stored.Add(s); });

        mock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        return (new UpdateVendorScoreCommandHandler(mock.Object), mock, stored);
    }

    private static UpdateVendorScoreCommand Cmd(
        Guid? vendorId = null, bool onTime = true, bool qualityIssues = false)
        => new(vendorId ?? Guid.NewGuid(), onTime, qualityIssues, HasDiscrepancy: qualityIssues);

    // Scoring formula:
    // delivery = onTime ? 100 : 60
    // quality  = issues ? 50  : 100
    // price    = 80 (static), doc = 80 (static)
    // total    = (delivery + quality + 80 + 80) / 4

    [Theory]
    [InlineData(true,  false, 90.0,  VendorTier.Gold)]    // (100+100+80+80)/4 = 90
    [InlineData(false, false, 80.0,  VendorTier.Silver)]   // ( 60+100+80+80)/4 = 80
    [InlineData(true,  true,  77.5,  VendorTier.Silver)]   // (100+ 50+80+80)/4 = 77.5
    [InlineData(false, true,  67.5,  VendorTier.Bronze)]   // ( 60+ 50+80+80)/4 = 67.5
    public async Task Score_CorrectTierAndTotal_ForAllCombinations(
        bool deliveredOnTime, bool hasQualityIssues,
        double expectedTotal, VendorTier expectedTier)
    {
        var (handler, _, stored) = BuildHandler();

        await handler.Handle(Cmd(onTime: deliveredOnTime, qualityIssues: hasQualityIssues), CancellationToken.None);

        stored.Should().HaveCount(1);
        stored[0].TotalScore.Should().Be((decimal)expectedTotal);
        stored[0].Tier.Should().Be(expectedTier);
    }

    [Fact]
    public async Task Score_AllPerfect_ShouldBeGold90_WithCorrectComponents()
    {
        var (handler, _, stored) = BuildHandler();

        await handler.Handle(Cmd(onTime: true, qualityIssues: false), CancellationToken.None);

        stored[0].DeliveryScore.Should().Be(100m);
        stored[0].QualityScore.Should().Be(100m);
        stored[0].TotalScore.Should().Be(90m);
        stored[0].Tier.Should().Be(VendorTier.Gold);
    }

    [Fact]
    public async Task Score_LateDeliveryNoIssues_ShouldBeSilver80()
    {
        var (handler, _, stored) = BuildHandler();

        await handler.Handle(Cmd(onTime: false, qualityIssues: false), CancellationToken.None);

        stored[0].TotalScore.Should().Be(80m);
        stored[0].Tier.Should().Be(VendorTier.Silver);
    }

    [Fact]
    public async Task Score_OnTimeWithQualityIssues_ShouldBeSilver77_5()
    {
        var (handler, _, stored) = BuildHandler();

        await handler.Handle(Cmd(onTime: true, qualityIssues: true), CancellationToken.None);

        stored[0].TotalScore.Should().Be(77.5m);
        stored[0].Tier.Should().Be(VendorTier.Silver);
    }

    [Fact]
    public async Task Score_LateWithQualityIssues_ShouldBeBronze67_5()
    {
        var (handler, _, stored) = BuildHandler();

        await handler.Handle(Cmd(onTime: false, qualityIssues: true), CancellationToken.None);

        stored[0].TotalScore.Should().Be(67.5m);
        stored[0].Tier.Should().Be(VendorTier.Bronze);
    }

    // ── Running average (same quarter update) ────────────────────────────────

    [Fact]
    public async Task Score_ExistingCurrentQuarter_ShouldRunningAverageDelivery()
    {
        var now = DateTime.UtcNow;
        var vendorId = Guid.NewGuid();
        var existing = new VendorScore
        {
            VendorId      = vendorId,
            PeriodYear    = now.Year,
            PeriodQuarter = (now.Month - 1) / 3 + 1,
            DeliveryScore = 100m,
            QualityScore  = 100m,
            PriceScore    = 80m,
            DocScore      = 80m,
            TotalScore    = 90m,
            Tier          = VendorTier.Gold,
        };

        var (handler, _, _) = BuildHandler(existing);

        // Second event: late delivery → delivery running avg = (100+60)/2 = 80
        await handler.Handle(Cmd(vendorId, onTime: false, qualityIssues: false), CancellationToken.None);

        existing.DeliveryScore.Should().Be(80m);
        existing.QualityScore.Should().Be(100m);
    }

    [Fact]
    public async Task Score_NoExistingRecord_ShouldCallAdd()
    {
        var (handler, mock, _) = BuildHandler(existing: null);

        await handler.Handle(Cmd(), CancellationToken.None);

        mock.Verify(r => r.Add(It.IsAny<VendorScore>()), Times.Once);
        mock.Verify(r => r.Update(It.IsAny<VendorScore>()), Times.Never);
    }
}
