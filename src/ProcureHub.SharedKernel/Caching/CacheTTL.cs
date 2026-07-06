namespace ProcureHub.SharedKernel.Caching;

public static class CacheTTL
{
    public static readonly TimeSpan UnitOfMeasures     = TimeSpan.FromDays(30);
    public static readonly TimeSpan PaymentTerms       = TimeSpan.FromDays(30);
    public static readonly TimeSpan CurrencyList       = TimeSpan.FromDays(7);
    public static readonly TimeSpan CurrencyById       = TimeSpan.FromDays(1);
    public static readonly TimeSpan Locations          = TimeSpan.FromDays(7);
    public static readonly TimeSpan MaterialCategories = TimeSpan.FromDays(7);
    public static readonly TimeSpan MaterialList       = TimeSpan.FromHours(6);
    public static readonly TimeSpan MaterialById       = TimeSpan.FromHours(12);
    public static readonly TimeSpan ApprovalPolicies   = TimeSpan.FromDays(1);
    public static readonly TimeSpan VendorList         = TimeSpan.FromMinutes(30);
    public static readonly TimeSpan VendorById         = TimeSpan.FromHours(1);
    public static readonly TimeSpan PRList             = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan PRById             = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan RFQList            = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan RFQById            = TimeSpan.FromMinutes(15);
}
