using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.VendorManagement.Application.Commands.DeleteVendorCapability;

public record DeleteVendorCapabilityCommand(Guid CapabilityId) : ICommand;
