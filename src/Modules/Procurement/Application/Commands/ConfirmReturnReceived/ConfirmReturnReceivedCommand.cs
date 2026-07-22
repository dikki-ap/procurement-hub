using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Commands.ConfirmReturnReceived;

public record ConfirmReturnReceivedCommand(Guid ReturnOrderId) : ICommand;
