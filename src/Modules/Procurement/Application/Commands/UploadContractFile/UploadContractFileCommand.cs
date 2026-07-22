using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Commands.UploadContractFile;

public record UploadContractFileCommand(
    Guid   ContractId,
    Stream FileStream,
    string FileName,
    string ContentType
) : ICommand<string>;
