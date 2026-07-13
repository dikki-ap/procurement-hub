using Microsoft.Extensions.Caching.Memory;
using Moq;
using ProcureHub.Modules.MasterData.Application.Commands.CreateCurrency;
using ProcureHub.Modules.MasterData.Application.Commands.UpdateCurrency;
using ProcureHub.Modules.MasterData.Application.Queries.GetCurrencyList;
using ProcureHub.Modules.MasterData.Application.Services;
using ProcureHub.Modules.MasterData.Domain.Entities;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.UnitTests.MasterData;

public class CurrencyCommandHandlerTests : IDisposable
{
    private readonly Mock<ICurrencyRepository>   _repoMock           = new();
    private readonly Mock<IExchangeRateService>  _exchangeRateMock   = new();
    private readonly MemoryCache                 _memoryCache        = new(new MemoryCacheOptions());
    private readonly MemoryCacheService          _cache;

    public CurrencyCommandHandlerTests() => _cache = new MemoryCacheService(_memoryCache);

    public void Dispose() => _memoryCache.Dispose();

    // ── CreateCurrencyCommandHandler ─────────────────────────────────────────

    [Fact]
    public async Task Create_WithValidData_ShouldReturnGuid()
    {
        _repoMock.Setup(r => r.ExistsByCodeAsync("USD", null, default)).ReturnsAsync(false);

        var handler = new CreateCurrencyCommandHandler(_repoMock.Object, _cache);
        var command = new CreateCurrencyCommand("USD", "US Dollar", "$", 16000m, false);

        var result = await handler.Handle(command, default);

        result.Should().NotBeEmpty();
        _repoMock.Verify(r => r.Add(It.IsAny<Currency>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Create_WithDuplicateCode_ShouldThrowConflictException()
    {
        _repoMock.Setup(r => r.ExistsByCodeAsync("USD", null, default)).ReturnsAsync(true);

        var handler = new CreateCurrencyCommandHandler(_repoMock.Object, _cache);
        var command = new CreateCurrencyCommand("USD", "US Dollar", "$", 16000m, false);

        var act = () => handler.Handle(command, default);

        await act.Should().ThrowAsync<ConflictException>();
        _repoMock.Verify(r => r.Add(It.IsAny<Currency>()), Times.Never);
    }

    [Fact]
    public async Task Create_ShouldStoreCodeAsUppercase()
    {
        _repoMock.Setup(r => r.ExistsByCodeAsync("usd", null, default)).ReturnsAsync(false);
        Currency? captured = null;
        _repoMock.Setup(r => r.Add(It.IsAny<Currency>())).Callback<Currency>(c => captured = c);

        var handler = new CreateCurrencyCommandHandler(_repoMock.Object, _cache);
        await handler.Handle(new CreateCurrencyCommand("usd", "US Dollar", null, 16000m, false), default);

        captured!.Code.Should().Be("USD");
    }

    [Fact]
    public async Task Create_ShouldInvalidateCurrencyCache()
    {
        _repoMock.Setup(r => r.ExistsByCodeAsync(It.IsAny<string>(), null, default)).ReturnsAsync(false);
        _cache.Set(CacheKeys.Currencies.List, new object(), TimeSpan.FromMinutes(5));

        var handler = new CreateCurrencyCommandHandler(_repoMock.Object, _cache);
        await handler.Handle(new CreateCurrencyCommand("EUR", "Euro", "€", 17500m, false), default);

        _cache.Get<object>(CacheKeys.Currencies.List).Should().BeNull();
    }

    // ── UpdateCurrencyCommandHandler ─────────────────────────────────────────

    [Fact]
    public async Task Update_WithNonExistentId_ShouldThrowNotFoundException()
    {
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync((Currency?)null);

        var handler = new UpdateCurrencyCommandHandler(_repoMock.Object, _cache, _exchangeRateMock.Object);
        var command = new UpdateCurrencyCommand(id, "USD", "US Dollar", "$", 16000m, false, true);

        var act = () => handler.Handle(command, default);

        await act.Should().ThrowAsync<NotFoundException>();
        _repoMock.Verify(r => r.Update(It.IsAny<Currency>()), Times.Never);
    }

    [Fact]
    public async Task Update_WithDuplicateCode_ShouldThrowConflictException()
    {
        var id       = Guid.NewGuid();
        var existing = new Currency { Code = "IDR", Name = "Rupiah", ExchangeRate = 1m, IsBase = true };
        _repoMock.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync(existing);
        _repoMock.Setup(r => r.ExistsByCodeAsync("USD", id, default)).ReturnsAsync(true);

        var handler = new UpdateCurrencyCommandHandler(_repoMock.Object, _cache, _exchangeRateMock.Object);
        var command = new UpdateCurrencyCommand(id, "USD", "US Dollar", "$", 16000m, false, true);

        var act = () => handler.Handle(command, default);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Update_ShouldInvalidateCurrencyCache()
    {
        var id       = Guid.NewGuid();
        var existing = new Currency { Code = "IDR", Name = "Rupiah", ExchangeRate = 1m, IsBase = true };
        _repoMock.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync(existing);
        _repoMock.Setup(r => r.ExistsByCodeAsync(It.IsAny<string>(), id, default)).ReturnsAsync(false);
        _cache.Set(CacheKeys.Currencies.List, new object(), TimeSpan.FromMinutes(5));

        var handler = new UpdateCurrencyCommandHandler(_repoMock.Object, _cache, _exchangeRateMock.Object);
        await handler.Handle(new UpdateCurrencyCommand(id, "IDR", "Indonesian Rupiah", "Rp", 1m, true, true), default);

        _cache.Get<object>(CacheKeys.Currencies.List).Should().BeNull();
    }

    // ── GetCurrencyListQueryHandler ──────────────────────────────────────────

    [Fact]
    public async Task GetList_OnCacheMiss_ShouldCallRepository()
    {
        _repoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync([]);

        var handler = new GetCurrencyListQueryHandler(_repoMock.Object, _cache);
        await handler.Handle(new GetCurrencyListQuery(), default);

        _repoMock.Verify(r => r.GetAllAsync(default), Times.Once);
    }

    [Fact]
    public async Task GetList_OnCacheHit_ShouldNotCallRepositoryTwice()
    {
        _repoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync([]);

        var handler = new GetCurrencyListQueryHandler(_repoMock.Object, _cache);
        await handler.Handle(new GetCurrencyListQuery(), default);
        await handler.Handle(new GetCurrencyListQuery(), default);

        _repoMock.Verify(r => r.GetAllAsync(default), Times.Once);
    }
}
