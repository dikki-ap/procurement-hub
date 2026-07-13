using ProcureHub.Modules.ApprovalEngine.Application.DTOs;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.ApprovalEngine.Application.Queries.GetApproverMatrix;

public record GetApproverMatrixQuery(Guid CompanyId) : IQuery<List<ApproverMatrixEntryDto>>;
