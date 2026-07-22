using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.Abstractions;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetContractDownloadUrl;

public class GetContractDownloadUrlQueryHandler : IQueryHandler<GetContractDownloadUrlQuery, string>
{
    private const string Bucket = "contracts";

    private readonly IContractRepository _repo;
    private readonly IStorageService     _storage;

    public GetContractDownloadUrlQueryHandler(IContractRepository repo, IStorageService storage)
    {
        _repo    = repo;
        _storage = storage;
    }

    public async Task<string> Handle(GetContractDownloadUrlQuery query, CancellationToken ct)
    {
        var contract = await _repo.GetByIdAsync(query.ContractId, ct)
            ?? throw new NotFoundException("Contract", query.ContractId);

        if (string.IsNullOrEmpty(contract.FileKey))
            throw new BusinessRuleException("ContractDownload", "No file has been uploaded for this contract.");

        return await _storage.GetPresignedUrlAsync(
            Bucket,
            contract.FileKey,
            TimeSpan.FromMinutes(30),
            $"attachment; filename=\"{contract.ContractNumber}.pdf\"",
            ct);
    }
}
