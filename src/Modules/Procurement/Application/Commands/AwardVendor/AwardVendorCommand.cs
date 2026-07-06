using MediatR;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Commands.AwardVendor;

public record AwardVendorCommand(
    Guid RFQId,
    Guid QuotationId,
    Guid VendorId) : ICommand;
