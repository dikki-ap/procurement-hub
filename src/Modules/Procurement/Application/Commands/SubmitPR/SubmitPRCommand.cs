using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Commands.SubmitPR;

public record SubmitPRCommand(Guid Id) : ICommand;
