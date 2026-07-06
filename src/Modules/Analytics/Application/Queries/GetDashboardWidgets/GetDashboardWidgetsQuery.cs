using MediatR;

namespace ProcureHub.Modules.Analytics.Application.Queries.GetDashboardWidgets;

public record GetDashboardWidgetsQuery(Guid CompanyId, string Role, Guid? VendorId = null)
    : IRequest<object>;
