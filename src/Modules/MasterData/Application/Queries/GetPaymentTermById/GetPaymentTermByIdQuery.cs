using ProcureHub.Modules.MasterData.Application.DTOs;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Queries.GetPaymentTermById;

public record GetPaymentTermByIdQuery(Guid Id) : IQuery<PaymentTermDto>;
