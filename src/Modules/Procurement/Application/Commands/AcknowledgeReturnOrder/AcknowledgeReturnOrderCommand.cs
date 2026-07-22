using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Commands.AcknowledgeReturnOrder;

public record AcknowledgeReturnOrderCommand(Guid ReturnOrderId, Guid VendorId) : ICommand;
