using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.CompanyManagement.Application.Commands.UploadCompanyLogo;

public record UploadCompanyLogoCommand(
    Guid    CompanyId,
    Stream  FileStream,
    string  FileName,
    string  ContentType
) : ICommand<string>;
