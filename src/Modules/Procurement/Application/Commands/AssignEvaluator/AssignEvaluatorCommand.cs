using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Commands.AssignEvaluator;

public record AssignEvaluatorCommand(
    Guid   RFQId,
    Guid   UserId,
    string UserName) : ICommand<Guid>;
