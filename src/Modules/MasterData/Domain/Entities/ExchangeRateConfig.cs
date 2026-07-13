using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.MasterData.Domain.Entities;

public class ExchangeRateConfig : BaseEntity
{
    public bool AutoSync { get; set; } = true;
}
