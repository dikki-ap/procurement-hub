using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.VendorManagement.Application.Queries.GetMyVendorId;

public record GetMyVendorIdQuery(string KeycloakId) : IQuery<Guid>;
