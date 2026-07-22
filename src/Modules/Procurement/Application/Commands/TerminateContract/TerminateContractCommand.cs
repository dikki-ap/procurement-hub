using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Commands.TerminateContract;

public record TerminateContractCommand(Guid Id, string? Reason = null) : ICommand;
