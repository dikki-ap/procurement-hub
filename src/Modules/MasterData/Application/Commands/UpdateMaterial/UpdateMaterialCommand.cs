using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Commands.UpdateMaterial;

public record UpdateMaterialCommand(
    Guid     Id,
    Guid     CategoryId,
    string   Code,
    string   Name,
    string?  Description,
    Guid     UomId,
    decimal? EstimatedPrice,
    Guid?    CurrencyId,
    bool     IsStrategic,
    bool     IsActive
) : ICommand;
