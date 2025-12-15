using Discord;
using Moq;
using Watcher.Runner.DiscordEventHandlers;
using Watcher.Runner.Logging;

namespace Watcher.Runner.Tests.Unit;

[TestFixture]
public class LogHandlerTests
{
    private Mock<IEventLogger> _eventLoggerMock;
    private LogHandler _sut;

    [SetUp]
    public void SetUp()
    {
        _eventLoggerMock = new Mock<IEventLogger>();
        _sut = new LogHandler(_eventLoggerMock.Object);
    }

    [Test]
    public async Task Handle_ShouldCallLogMessageOnEventLogger()
    {
        // Arrange
        var logMessage = new LogMessage(LogSeverity.Info, "TestSource", "Test message");

        // Act
        await _sut.Handle(logMessage);

        // Assert
        _eventLoggerMock.Verify(x => x.LogMessage(logMessage), Times.Once);
    }

    [Test]
    public async Task Handle_ShouldPassExactLogMessageToEventLogger()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");
        var logMessage = new LogMessage(LogSeverity.Error, "ErrorSource", "Error occurred", exception);

        // Act
        await _sut.Handle(logMessage);

        // Assert
        _eventLoggerMock.Verify(
            x => x.LogMessage(It.Is<LogMessage>(m =>
                m.Severity == LogSeverity.Error &&
                m.Source == "ErrorSource" &&
                m.Message == "Error occurred" &&
                m.Exception == exception)),
            Times.Once);
    }

    [Test]
    public void Handle_ShouldReturnCompletedTask()
    {
        // Arrange
        var logMessage = new LogMessage(LogSeverity.Debug, "Source", "Message");

        // Act
        var result = _sut.Handle(logMessage);

        // Assert
        Assert.That(result.IsCompleted, Is.True);
    }

    [TestCase(LogSeverity.Critical)]
    [TestCase(LogSeverity.Error)]
    [TestCase(LogSeverity.Warning)]
    [TestCase(LogSeverity.Info)]
    [TestCase(LogSeverity.Verbose)]
    [TestCase(LogSeverity.Debug)]
    public async Task Handle_ShouldLogMessageWithAnySeverity(LogSeverity severity)
    {
        // Arrange
        var logMessage = new LogMessage(severity, "Source", "Message");

        // Act
        await _sut.Handle(logMessage);

        // Assert
        _eventLoggerMock.Verify(
            x => x.LogMessage(It.Is<LogMessage>(m => m.Severity == severity)),
            Times.Once);
    }
}
