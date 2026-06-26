using ProcureHub.Modules.MasterData.Application.DTOs;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Queries.GetPaymentTermList;

public record GetPaymentTermListQuery(Guid CompanyId) : IQuery<List<PaymentTermDto>>;
