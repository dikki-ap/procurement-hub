using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetContractDownloadUrl;

public record GetContractDownloadUrlQuery(Guid ContractId) : IQuery<string>;
