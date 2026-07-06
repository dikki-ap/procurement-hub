using MediatR;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Commands.WithdrawQuotation;

public record WithdrawQuotationCommand(Guid QuotationId) : ICommand;
