using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ProcureHub.Modules.VendorManagement.Application.Commands.RegisterVendor;
using ProcureHub.Modules.VendorManagement.Domain.Enums;
using ProcureHub.SharedKernel.Common;

namespace ProcureHub.API.Controllers.v1.VendorManagement;

/// <summary>Self-registration endpoint — open to unauthenticated vendors.</summary>
[ApiController]
[Route("api/v1/vendor-registration")]
[AllowAnonymous]
[EnableRateLimiting("vendor-register")]
public class VendorRegistrationController : ControllerBase
{
    private readonly IMediator _mediator;

    public VendorRegistrationController(IMediator mediator) => _mediator = mediator;

    /// <summary>Register a new vendor.</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<object>>> Register(
        [FromBody] RegisterVendorRequest request, CancellationToken ct)
    {
        var id = await _mediator.Send(new RegisterVendorCommand(
            request.CompanyId,
            request.LegalName,
            request.TradeName,
            request.VendorType,
            request.Npwp,
            request.Siup,
            request.Nib,
            request.Address,
            request.City,
            request.Province,
            request.PostalCode,
            request.Country,
            request.ContactName,
            request.ContactPosition,
            request.ContactEmail,
            request.ContactPhone), ct);

        return Ok(ApiResponse.Ok(new { id, message = "Registration submitted. You will be contacted once approved." }));
    }
}

public record RegisterVendorRequest(
    Guid       CompanyId,
    string     LegalName,
    string?    TradeName,
    VendorType VendorType,
    string?    Npwp,
    string?    Siup,
    string?    Nib,
    string?    Address,
    string?    City,
    string?    Province,
    string?    PostalCode,
    string?    Country,
    string     ContactName,
    string?    ContactPosition,
    string     ContactEmail,
    string?    ContactPhone
);
