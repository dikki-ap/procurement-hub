using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Commands.ActivateContract;

public record ActivateContractCommand(Guid Id) : ICommand;
