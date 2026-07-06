using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Commands.OpenRFQ;

public record OpenRFQCommand(Guid Id) : ICommand;
