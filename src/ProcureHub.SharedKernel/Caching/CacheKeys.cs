namespace ProcureHub.SharedKernel.Caching;

public static class CacheKeys
{
    public static class UnitOfMeasures
    {
        public const string Prefix = "uom:";
        public const string List   = "uom:list";
        public static string ById(Guid id) => $"uom:{id}";
    }

    public static class PaymentTerms
    {
        public const string Prefix = "payment-terms:";
        public const string List   = "payment-terms:list";
        public static string ById(Guid id) => $"payment-terms:{id}";
    }

    public static class Currencies
    {
        public const string Prefix = "currencies:";
        public const string List   = "currencies:list";
        public static string ById(Guid id) => $"currencies:{id}";
    }

    public static class Locations
    {
        public const string Prefix = "locations:";
        public const string List   = "locations:list";
        public static string ById(Guid id) => $"locations:{id}";
    }

    public static class MaterialCategories
    {
        public const string Prefix = "material-categories:";
        public const string List   = "material-categories:list";
        public static string ById(Guid id) => $"material-categories:{id}";
    }

    public static class Materials
    {
        public const string Prefix = "materials:";
        public const string List   = "materials:list";
        public static string ById(Guid id) => $"materials:{id}";
    }

    public static class ApprovalPolicies
    {
        public const string Prefix = "approval-policies:";
        public const string List   = "approval-policies:list";
    }

    public static class Vendors
    {
        public const string Prefix = "vendors:";
        public const string List   = "vendors:list";
        public static string ById(Guid id) => $"vendors:{id}";
    }

    public static class PurchaseRequisitions
    {
        public const string Prefix = "prs:";
        public static string List(Guid companyId) => $"prs:list:{companyId}";
        public static string ById(Guid id)        => $"prs:{id}";
    }

    public static class RFQs
    {
        public const string Prefix = "rfqs:";
        public static string List(Guid companyId) => $"rfqs:list:{companyId}";
        public static string ById(Guid id)        => $"rfqs:{id}";
    }
}
