using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Commands.SubmitQuotation;

public record SubmitQuotationItemInput(
    Guid    RFQItemId,
    decimal UnitPrice,
    string? Notes = null);

public record SubmitQuotationCommand(
    Guid                          RFQId,
    Guid                          VendorId,
    string?                       Notes,
    List<SubmitQuotationItemInput> Items) : ICommand<Guid>;
