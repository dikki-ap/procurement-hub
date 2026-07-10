using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Commands.DeleteDocumentType;

public record DeleteDocumentTypeCommand(Guid Id) : ICommand;
