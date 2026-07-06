namespace ProcureHub.Modules.Analytics.Application.Queries.GetDashboardWidgets;

public record DashboardWidgetsDto(
    decimal SpendThisMonth,
    int     PendingApprovals,
    int     OpenRFQs,
    int     ActivePOs,
    int     PendingInvoices,
    int     TotalVendors,
    int     TotalPRs
);

public record VendorDashboardWidgetsDto(
    int     MyActivePOs,
    int     MyPendingInvoices,
    decimal MyLatestScore,
    string  MyTier
);
