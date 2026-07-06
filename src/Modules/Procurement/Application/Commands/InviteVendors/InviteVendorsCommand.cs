using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Commands.InviteVendors;

public record InviteVendorsCommand(Guid RFQId, List<Guid> VendorIds) : ICommand;
