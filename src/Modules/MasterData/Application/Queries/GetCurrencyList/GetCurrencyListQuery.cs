using ProcureHub.Modules.MasterData.Application.DTOs;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Queries.GetCurrencyList;

public record GetCurrencyListQuery : IQuery<List<CurrencyDto>>;
