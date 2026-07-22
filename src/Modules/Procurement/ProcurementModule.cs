using Microsoft.Extensions.DependencyInjection;
using ProcureHub.Modules.Procurement.Application.Services;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.Modules.Procurement.Infrastructure.Jobs;
using ProcureHub.Modules.Procurement.Infrastructure.Repositories;
using ProcureHub.Modules.Procurement.Infrastructure.Services;

namespace ProcureHub.Modules.Procurement;

public static class ProcurementModule
{
    public static IServiceCollection AddProcurementServices(this IServiceCollection services)
    {
        services.AddScoped<IPurchaseRequisitionRepository, PurchaseRequisitionRepository>();
        services.AddScoped<IRFQRepository,                 RFQRepository>();
        services.AddScoped<IVendorQuotationRepository,     VendorQuotationRepository>();
        services.AddScoped<IBidEvaluationRepository,       BidEvaluationRepository>();
        services.AddScoped<IPurchaseOrderRepository,       PurchaseOrderRepository>();
        services.AddScoped<IGoodsReceiptRepository,        GoodsReceiptRepository>();
        services.AddScoped<IInvoiceRepository,             InvoiceRepository>();
        services.AddScoped<IContractRepository,            ContractRepository>();
        services.AddScoped<IReturnOrderRepository,         ReturnOrderRepository>();

        services.AddScoped<IVendorChecker,                 VendorChecker>();

        services.AddScoped<BidDeadlineReminderJob>();
        services.AddScoped<ContractExpiryReminderJob>();

        return services;
    }
}
