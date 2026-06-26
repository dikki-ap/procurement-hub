using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Commands.DeleteMaterial;

public record DeleteMaterialCommand(Guid Id) : ICommand;
