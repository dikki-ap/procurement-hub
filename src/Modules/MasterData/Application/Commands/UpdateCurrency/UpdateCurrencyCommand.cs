using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Commands.UpdateCurrency;

public record UpdateCurrencyCommand(
    Guid    Id,
    string  Code,
    string  Name,
    string? Symbol,
    decimal ExchangeRate,
    bool    IsBase,
    bool    IsActive
) : ICommand;
