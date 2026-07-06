using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Commands.ConfirmGRN;

public record ConfirmGRNCommand(Guid GRNId, Guid VendorId) : ICommand;
