using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Commands.CancelPR;

public record CancelPRCommand(Guid Id) : ICommand;
