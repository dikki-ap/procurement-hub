namespace ProcureHub.Modules.Procurement.Domain.Enums;

public enum POStatus
{
    Draft           = 0,
    PendingApproval = 1,
    Approved        = 2,
    Issued          = 3,
    Acknowledged    = 4,
    InDelivery      = 5,
    Completed       = 6,
    Cancelled       = 7,
}
