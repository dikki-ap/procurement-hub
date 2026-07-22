using ProcureHub.Modules.VendorManagement.Application.DTOs;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.VendorManagement.Application.Queries.GetVendorScoreHistory;

public record GetVendorScoreHistoryQuery(Guid VendorId) : IQuery<List<VendorScoreDto>>;
