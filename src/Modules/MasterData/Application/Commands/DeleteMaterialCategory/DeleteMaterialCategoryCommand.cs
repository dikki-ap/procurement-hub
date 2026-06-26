using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Commands.DeleteMaterialCategory;

public record DeleteMaterialCategoryCommand(Guid Id) : ICommand;
