using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Commands.DeletePaymentTerm;

public record DeletePaymentTermCommand(Guid Id) : ICommand;
