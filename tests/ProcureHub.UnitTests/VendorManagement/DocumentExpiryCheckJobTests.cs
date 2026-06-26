using Microsoft.Extensions.Logging;
using Moq;
using ProcureHub.Modules.VendorManagement.Domain.Entities;
using ProcureHub.Modules.VendorManagement.Domain.Enums;
using ProcureHub.Modules.VendorManagement.Domain.Repositories;
using ProcureHub.Modules.VendorManagement.Infrastructure.Jobs;

namespace ProcureHub.UnitTests.VendorManagement;

public class DocumentExpiryCheckJobTests
{
    private readonly Mock<IVendorDocumentRepository>     _docRepoMock = new();
    private readonly Mock<ILogger<DocumentExpiryCheckJob>> _loggerMock  = new();

    private DocumentExpiryCheckJob CreateJob()
        => new(_docRepoMock.Object, _loggerMock.Object);

    [Fact]
    public async Task Execute_WithNoExpiringDocuments_ShouldNotSaveChanges()
    {
        _docRepoMock.Setup(r => r.GetExpiringAsync(30, default)).ReturnsAsync([]);

        await CreateJob().ExecuteAsync();

        _docRepoMock.Verify(r => r.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task Execute_WithExpiredDocument_ShouldMarkAsExpired()
    {
        var doc = new VendorDocument
        {
            VendorId     = Guid.NewGuid(),
            DocumentType = DocumentType.Npwp,
            FileUrl      = "vendor-documents/test.pdf",
            Status       = DocumentStatus.Active,
            ExpiredAt    = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), // already expired
        };

        _docRepoMock.Setup(r => r.GetExpiringAsync(30, default)).ReturnsAsync([doc]);

        await CreateJob().ExecuteAsync();

        doc.Status.Should().Be(DocumentStatus.Expired);
        _docRepoMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Execute_WithDocumentExpiringSoon_ShouldNotChangeStatus()
    {
        var doc = new VendorDocument
        {
            VendorId     = Guid.NewGuid(),
            DocumentType = DocumentType.Siup,
            FileUrl      = "vendor-documents/test.pdf",
            Status       = DocumentStatus.Active,
            ExpiredAt    = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15)), // expiring in 15 days
        };

        _docRepoMock.Setup(r => r.GetExpiringAsync(30, default)).ReturnsAsync([doc]);

        await CreateJob().ExecuteAsync();

        doc.Status.Should().Be(DocumentStatus.Active); // not expired yet
        _docRepoMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }
}
