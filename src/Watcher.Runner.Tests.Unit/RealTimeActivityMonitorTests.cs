using Moq;
using Watcher.Runner.DiscordEventHandlers;
using Watcher.Runner.Monitoring;
using Watcher.Runner.Providers;
using Watcher.Runner.Storage;

namespace Watcher.Runner.Tests.Unit;

[TestFixture]
public class RealTimeActivityMonitorTests
{
    private Mock<IEventBus> _eventBusMock;
    private Mock<IDateTimeProvider> _dateTimeProviderMock;
    private Mock<IMessagesStorage> _messagesStorageMock;
    private RealTimeActivityMonitor _sut;

    private DateTime _now;
    private Action<MessageInfoReceivedEvent>? _messageReceivedHandler;

    [SetUp]
    public void SetUp()
    {
        this._eventBusMock = new Mock<IEventBus>();
        this._dateTimeProviderMock = new Mock<IDateTimeProvider>();
        this._messagesStorageMock = new Mock<IMessagesStorage>();

        this._now = new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc);
        _ = this._dateTimeProviderMock.Setup(x => x.GetUtcNow()).Returns(() => this._now);
        _ = this._messagesStorageMock.Setup(x => x.GetMessagesInfos(It.IsAny<ulong?>(), It.IsAny<ulong?>(), It.IsAny<DateTime?>())).ReturnsAsync([]);

        _ = this._eventBusMock
            .Setup(x => x.Subscribe(It.IsAny<Action<MessageInfoReceivedEvent>>()))
            .Callback<Action<MessageInfoReceivedEvent>>(h => this._messageReceivedHandler = h);

        this._sut = new RealTimeActivityMonitor(
            this._eventBusMock.Object,
            this._dateTimeProviderMock.Object,
            this._messagesStorageMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        this._sut.Dispose();
    }

    private MessageInfo CreateMessage(DateTime sentAt) => new(ServerId: 1, ChannelId: 1, UserId: 1, MessageId: 1, SentAt: sentAt);

    private void SimulateMessageReceived(DateTime sentAt) => this._messageReceivedHandler?.Invoke(new MessageInfoReceivedEvent(this.CreateMessage(sentAt)));

    private void SetupStorageMessages(params DateTime[] sentAtTimes)
    {
        var messages = sentAtTimes.Select(this.CreateMessage).ToArray();
        _ = this._messagesStorageMock.Setup(x => x.GetMessagesInfos(It.IsAny<ulong?>(), It.IsAny<ulong?>(), It.IsAny<DateTime?>())).Returns(Task.FromResult(messages));
    }

    [Test]
    public async Task Initialize_LoadsRecentMessagesFromStorage()
    {
        this.SetupStorageMessages(this._now.AddMinutes(-30));

        await this._sut.Initialize();

        Assert.That(this._sut.GetLastHourMessagesCount(), Is.EqualTo(1));
    }

    [Test]
    public async Task Initialize_IgnoresOldMessagesFromStorage()
    {
        this.SetupStorageMessages(this._now.AddHours(-2));

        await this._sut.Initialize();

        Assert.That(this._sut.GetLastHourMessagesCount(), Is.Zero);
    }

    [Test]
    public async Task Initialize_TracksNewMessagesFromEventBus()
    {
        await this._sut.Initialize();

        this.SimulateMessageReceived(this._now.AddSeconds(-10));

        Assert.That(this._sut.GetLastHourMessagesCount(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetLastHourMessages_ReturnsRecentMessages()
    {
        this.SetupStorageMessages(this._now.AddMinutes(-30), this._now.AddMinutes(-45));
        await this._sut.Initialize();

        var result = this._sut.GetLastHourMessages();

        Assert.That(result, Has.Length.EqualTo(2));
    }

    [Test]
    public async Task GetLastHourMessages_ExcludesOutdatedMessagesAfterTimePass()
    {
        this.SetupStorageMessages(this._now.AddMinutes(-50));
        await this._sut.Initialize();

        this._now = this._now.AddMinutes(20);

        Assert.That(this._sut.GetLastHourMessagesCount(), Is.Zero);
    }

    [Test]
    public async Task GetHalfLastHourMessages_ReturnsOnlyLast30Minutes()
    {
        this.SetupStorageMessages(this._now.AddMinutes(-15), this._now.AddMinutes(-45));
        await this._sut.Initialize();

        var result = this._sut.GetHalfLastHourMessages();

        Assert.That(result, Has.Length.EqualTo(1));
    }

    [Test]
    public async Task GetHalfLastHourMessagesCount_ReturnsCorrectCount()
    {
        this.SetupStorageMessages(this._now.AddMinutes(-10), this._now.AddMinutes(-20), this._now.AddMinutes(-40));
        await this._sut.Initialize();

        var result = this._sut.GetHalfLastHourMessagesCount();

        Assert.That(result, Is.EqualTo(2));
    }
}