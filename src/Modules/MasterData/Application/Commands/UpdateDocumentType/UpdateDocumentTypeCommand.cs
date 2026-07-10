using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Commands.UpdateDocumentType;

public record UpdateDocumentTypeCommand(
    Guid    Id,
    string  Name,
    bool    IsActive,
    string? AllowedExtensions,
    int     MaxFileSizeMb
) : ICommand;
