using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Commands.CreateDocumentType;

public record CreateDocumentTypeCommand(
    string  Name,
    string? AllowedExtensions,
    int     MaxFileSizeMb
) : ICommand<Guid>;
