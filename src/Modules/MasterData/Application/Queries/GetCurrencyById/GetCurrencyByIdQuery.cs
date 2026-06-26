using ProcureHub.Modules.MasterData.Application.DTOs;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Queries.GetCurrencyById;

public record GetCurrencyByIdQuery(Guid Id) : IQuery<CurrencyDto>;
