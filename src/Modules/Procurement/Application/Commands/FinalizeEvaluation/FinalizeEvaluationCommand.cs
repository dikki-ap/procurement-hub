using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Commands.FinalizeEvaluation;

public record FinalizeEvaluationCommand(Guid RFQId) : ICommand;
