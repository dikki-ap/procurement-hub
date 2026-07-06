using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ProcureHub.Modules.Notifications.Domain;
using ProcureHub.Modules.Notifications.Domain.Repositories;
using ProcureHub.Modules.Notifications.Infrastructure.Hubs;
using ProcureHub.Modules.Notifications.Infrastructure.Services;

namespace ProcureHub.UnitTests.Notifications;

public class NotificationServiceTests
{
    private static (
        InAppNotificationService service,
        Mock<IInAppNotificationRepository> repoMock,
        Mock<IHubContext<NotificationHub>> hubMock,
        List<InAppNotification> stored)
        BuildService()
    {
        var stored   = new List<InAppNotification>();
        var repoMock = new Mock<IInAppNotificationRepository>();
        var hubMock  = new Mock<IHubContext<NotificationHub>>();

        repoMock.Setup(r => r.Add(It.IsAny<InAppNotification>()))
            .Callback<InAppNotification>(stored.Add);
        repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Wire up mock hub clients
        var clientsMock = new Mock<IHubClients>();
        var groupMock   = new Mock<IClientProxy>();
        hubMock.Setup(h => h.Clients).Returns(clientsMock.Object);
        clientsMock.Setup(c => c.Group(It.IsAny<string>())).Returns(groupMock.Object);
        groupMock.Setup(g => g.SendCoreAsync(
                It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new InAppNotificationService(
            repoMock.Object, hubMock.Object, NullLogger<InAppNotificationService>.Instance);

        return (service, repoMock, hubMock, stored);
    }

    [Fact]
    public async Task SendAsync_ShouldSaveNotificationToRepository()
    {
        var (service, _, _, stored) = BuildService();
        var userId = Guid.NewGuid();

        await service.SendAsync(userId, "Test Title", "Test message", "/app/test");

        stored.Should().HaveCount(1);
        stored[0].UserId.Should().Be(userId);
        stored[0].Title.Should().Be("Test Title");
        stored[0].Message.Should().Be("Test message");
        stored[0].Link.Should().Be("/app/test");
        stored[0].IsRead.Should().BeFalse();
    }

    [Fact]
    public async Task SendAsync_ShouldPushToCorrectSignalRGroup()
    {
        var (service, _, hubMock, _) = BuildService();
        var userId = Guid.NewGuid();

        await service.SendAsync(userId, "Title", "Msg");

        var expectedGroup = $"user:{userId}";
        hubMock.Verify(h => h.Clients.Group(expectedGroup), Times.Once);
    }

    [Fact]
    public async Task SendAsync_ShouldSendReceiveNotificationEvent()
    {
        var (service, _, hubMock, _) = BuildService();

        var clientsMock = new Mock<IHubClients>();
        var groupMock   = new Mock<IClientProxy>();
        hubMock.Setup(h => h.Clients).Returns(clientsMock.Object);
        clientsMock.Setup(c => c.Group(It.IsAny<string>())).Returns(groupMock.Object);
        groupMock.Setup(g => g.SendCoreAsync(
                It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await service.SendAsync(Guid.NewGuid(), "Title", "Msg");

        groupMock.Verify(g => g.SendCoreAsync(
            "ReceiveNotification",
            It.Is<object[]>(args => args.Length == 1),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendAsync_WithNullLink_ShouldStillPersistNotification()
    {
        var (service, _, _, stored) = BuildService();

        await service.SendAsync(Guid.NewGuid(), "Title", "Msg", link: null);

        stored[0].Link.Should().BeNull();
    }
}
