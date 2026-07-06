using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Commands.CreatePR;

public record CreatePRItemRequest(
    Guid?   MaterialId,
    string  ItemDescription,
    decimal Quantity,
    Guid?   UnitOfMeasureId,
    string? UnitLabel,
    decimal EstimatedUnitPrice,
    string? Notes);

public record CreatePRCommand(
    Guid                        CompanyId,
    string                      Title,
    string?                     Description,
    string                      Department,
    string?                     DeliveryLocation,
    DateTime                    RequiredDate,
    string?                     Notes,
    List<CreatePRItemRequest>   Items) : ICommand<Guid>;
