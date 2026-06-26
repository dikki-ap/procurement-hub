using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Commands.CreateMaterial;

public record CreateMaterialCommand(
    Guid     CategoryId,
    string   Code,
    string   Name,
    string?  Description,
    Guid     UomId,
    decimal? EstimatedPrice,
    Guid?    CurrencyId,
    bool     IsStrategic
) : ICommand<Guid>;
