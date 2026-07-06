using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.VendorManagement.Application.Commands.UpdateVendorScore;

public record UpdateVendorScoreCommand(
    Guid    VendorId,
    bool    DeliveredOnTime,
    bool    HasQualityIssues,
    bool    HasDiscrepancy
) : ICommand;
