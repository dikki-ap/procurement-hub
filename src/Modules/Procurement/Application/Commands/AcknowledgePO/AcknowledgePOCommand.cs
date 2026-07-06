using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Commands.AcknowledgePO;

public record AcknowledgePOCommand(Guid POId) : ICommand;
