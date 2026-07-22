using MediatR;
using Microsoft.Extensions.Logging;
using ProcureHub.Modules.Procurement.Domain.Events;
using ProcureHub.Modules.VendorManagement.Domain.Repositories;
using ProcureHub.SharedKernel.Abstractions;
using ProcureHub.SharedKernel.Notifications;

namespace ProcureHub.Modules.Procurement.Infrastructure.EventHandlers;

public class ContractActivatedEventHandler : INotificationHandler<ContractActivatedEvent>
{
    private readonly IVendorRepository                      _vendorRepo;
    private readonly IEmailService                          _email;
    private readonly ILogger<ContractActivatedEventHandler> _logger;

    public ContractActivatedEventHandler(
        IVendorRepository                      vendorRepo,
        IEmailService                          email,
        ILogger<ContractActivatedEventHandler> logger)
    {
        _vendorRepo = vendorRepo;
        _email      = email;
        _logger     = logger;
    }

    public async Task Handle(ContractActivatedEvent notification, CancellationToken ct)
    {
        _logger.LogInformation(
            "Contract activated — ContractId: {ContractId}, Number: {Number}",
            notification.ContractId, notification.ContractNumber);

        var vendor = await _vendorRepo.GetByIdWithDetailsAsync(notification.VendorId, ct);
        if (vendor is null) return;

        var contact = vendor.Contacts.FirstOrDefault(c => c.IsPrimary) ?? vendor.Contacts.FirstOrDefault();
        if (contact?.Email is null) return;

        var html = EmailTemplates.ContractActivated(
            vendor.LegalName,
            notification.ContractNumber,
            notification.Title,
            notification.StartDate,
            notification.EndDate);

        await _email.SendAsync(
            contact.Email,
            $"Contract Activated — {notification.ContractNumber}",
            html, ct);
    }
}
