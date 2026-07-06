using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Commands.CloseRFQ;

public record CloseRFQCommand(Guid Id) : ICommand;
