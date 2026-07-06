using ProcureHub.Modules.Procurement.Domain.Enums;

namespace ProcureHub.Modules.Procurement.Application.DTOs;

public class PRListDto
{
    public Guid     Id                  { get; set; }
    public string   PRNumber            { get; set; } = string.Empty;
    public string   Title               { get; set; } = string.Empty;
    public string   Department          { get; set; } = string.Empty;
    public DateTime RequiredDate        { get; set; }
    public PRStatus Status              { get; set; }
    public decimal  TotalEstimatedValue { get; set; }
    public int      ItemCount           { get; set; }
    public DateTime CreatedAt           { get; set; }
}
