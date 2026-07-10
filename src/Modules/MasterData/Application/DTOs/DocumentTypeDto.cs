namespace ProcureHub.Modules.MasterData.Application.DTOs;

public record DocumentTypeDto(
    Guid    Id,
    string  Name,
    bool    IsActive,
    string? AllowedExtensions,
    int     MaxFileSizeMb
);
