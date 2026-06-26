using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Commands.DeleteCurrency;

public record DeleteCurrencyCommand(Guid Id) : ICommand;
