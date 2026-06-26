using MediatR;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Commands.CreateCurrency;

public record CreateCurrencyCommand(
    string  Code,
    string  Name,
    string? Symbol,
    decimal ExchangeRate,
    bool    IsBase
) : ICommand<Guid>;
