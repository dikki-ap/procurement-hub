using ProcureHub.Modules.Procurement.Domain.Enums;
using ProcureHub.Modules.Procurement.Domain.Events;
using ProcureHub.SharedKernel.Domain;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Domain.Entities;

public class Contract : AggregateRoot
{
    public Guid           CompanyId      { get; set; }
    public string         ContractNumber { get; set; } = string.Empty;
    public Guid?          POId           { get; set; }
    public Guid           VendorId       { get; set; }
    public string         Title          { get; set; } = string.Empty;
    public ContractStatus Status         { get; set; } = ContractStatus.Draft;
    public string?        FileKey        { get; set; }
    public DateTime?      SignedAt       { get; set; }
    public DateTime?      StartDate      { get; set; }
    public DateTime?      EndDate        { get; set; }
    public decimal?       Value          { get; set; }
    public Guid?          CurrencyId     { get; set; }
    public string?        Notes          { get; set; }

    public PurchaseOrder? PurchaseOrder { get; set; }

    public void Activate()
    {
        if (Status != ContractStatus.Draft)
            throw new BusinessRuleException("ContractActivate",
                $"Only draft contracts can be activated. Current status: {Status}");

        Status   = ContractStatus.Active;
        SignedAt = DateTime.UtcNow;

        AddDomainEvent(new ContractActivatedEvent(Id, ContractNumber, Title, VendorId, StartDate, EndDate));
    }

    public void Terminate(string? reason = null)
    {
        if (Status != ContractStatus.Active)
            throw new BusinessRuleException("ContractTerminate",
                $"Only active contracts can be terminated. Current status: {Status}");

        Status = ContractStatus.Terminated;
        if (reason is not null) Notes = string.IsNullOrWhiteSpace(Notes)
            ? $"Terminated: {reason}"
            : $"{Notes}\n\nTerminated: {reason}";

        AddDomainEvent(new ContractTerminatedEvent(Id, ContractNumber, Title, VendorId));
    }

    public void Expire()
    {
        if (Status != ContractStatus.Active)
            return;

        Status = ContractStatus.Expired;
    }
}
