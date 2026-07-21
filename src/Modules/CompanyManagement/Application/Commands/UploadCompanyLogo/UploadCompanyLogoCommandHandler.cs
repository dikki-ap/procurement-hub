using ProcureHub.Modules.CompanyManagement.Domain.Repositories;
using ProcureHub.SharedKernel.Abstractions;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.CompanyManagement.Application.Commands.UploadCompanyLogo;

public class UploadCompanyLogoCommandHandler : ICommandHandler<UploadCompanyLogoCommand, string>
{
    private readonly ICompanyRepository _repo;
    private readonly IStorageService    _storage;

    public UploadCompanyLogoCommandHandler(ICompanyRepository repo, IStorageService storage)
    {
        _repo    = repo;
        _storage = storage;
    }

    public async Task<string> Handle(UploadCompanyLogoCommand command, CancellationToken ct)
    {
        var company = await _repo.GetByIdAsync(command.CompanyId, ct)
            ?? throw new NotFoundException("Company", command.CompanyId);

        var ext       = Path.GetExtension(command.FileName).ToLowerInvariant();
        var objectKey = $"companies/{command.CompanyId}/logo/{Guid.NewGuid():N}{ext}";

        await _storage.EnsureBucketExistsAsync("company-assets", ct);
        await _storage.UploadAsync("company-assets", objectKey, command.FileStream, command.ContentType, ct);

        company.LogoUrl = objectKey;

        _repo.Update(company);
        await _repo.SaveChangesAsync(ct);

        return objectKey;
    }
}
