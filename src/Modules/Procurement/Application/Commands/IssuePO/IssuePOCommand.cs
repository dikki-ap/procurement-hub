using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Commands.IssuePO;

public record IssuePOCommand(Guid POId) : ICommand;
