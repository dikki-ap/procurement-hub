using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.Abstractions;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Application.Commands.UploadContractFile;

public class UploadContractFileCommandHandler : ICommandHandler<UploadContractFileCommand, string>
{
    private const string Bucket = "contracts";

    private readonly IContractRepository _repo;
    private readonly IStorageService     _storage;

    public UploadContractFileCommandHandler(IContractRepository repo, IStorageService storage)
    {
        _repo    = repo;
        _storage = storage;
    }

    public async Task<string> Handle(UploadContractFileCommand cmd, CancellationToken ct)
    {
        var contract = await _repo.GetByIdAsync(cmd.ContractId, ct)
            ?? throw new NotFoundException("Contract", cmd.ContractId);

        var ext       = Path.GetExtension(cmd.FileName);
        var objectKey = $"contracts/{contract.CompanyId}/{contract.Id}/{Guid.NewGuid()}{ext}";

        await _storage.EnsureBucketExistsAsync(Bucket, ct);
        var key = await _storage.UploadAsync(Bucket, objectKey, cmd.FileStream, cmd.ContentType, ct);

        // Delete old file if it exists
        if (!string.IsNullOrEmpty(contract.FileKey))
        {
            try { await _storage.DeleteAsync(Bucket, contract.FileKey, ct); }
            catch { /* tolerate if old file is already gone */ }
        }

        contract.FileKey = key;
        _repo.Update(contract);
        await _repo.SaveChangesAsync(ct);
        return key;
    }
}
