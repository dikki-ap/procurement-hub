using ProcureHub.Modules.Procurement.Domain.Enums;
using ProcureHub.Modules.Procurement.Domain.Events;
using ProcureHub.SharedKernel.Domain;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Domain.Entities;

public class PurchaseRequisition : AggregateRoot
{
    public Guid      CompanyId             { get; set; }
    public string    PRNumber              { get; set; } = string.Empty;
    public string    Title                 { get; set; } = string.Empty;
    public string?   Description           { get; set; }
    public string    Department            { get; set; } = string.Empty;
    public string?   DeliveryLocation      { get; set; }
    public DateTime  RequiredDate          { get; set; }
    public PRStatus  Status                { get; set; } = PRStatus.Draft;
    public decimal   TotalEstimatedValue   { get; set; }
    public string?   Notes                 { get; set; }
    public Guid      RequestedById         { get; set; }

    // Navigation
    public ICollection<PRItem> Items { get; set; } = [];

    // ── Domain methods ──────────────────────────────────────────────────────

    public void Submit()
    {
        if (Status != PRStatus.Draft)
            throw new BusinessRuleException("PRSubmit", $"Only draft PRs can be submitted. Current status: {Status}");

        if (!Items.Any())
            throw new BusinessRuleException("PRSubmit", "PR must have at least one item before submission.");

        Status = PRStatus.Submitted;
        AddDomainEvent(new PRSubmittedEvent(Id, PRNumber, RequestedById));
    }

    public void Cancel()
    {
        if (Status is PRStatus.Approved or PRStatus.Cancelled)
            throw new BusinessRuleException("PRCancel", $"PR cannot be cancelled from status: {Status}");

        Status = PRStatus.Cancelled;
        AddDomainEvent(new PRCancelledEvent(Id, PRNumber));
    }

    public void RecalculateTotalValue()
        => TotalEstimatedValue = Items.Sum(i => i.Quantity * i.EstimatedUnitPrice);
}
