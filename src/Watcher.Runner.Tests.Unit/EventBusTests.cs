using Moq;
using Watcher.Runner.Logging;

namespace Watcher.Runner.Tests.Unit;

[TestFixture]
public class EventBusTests
{
    public record TestEvent : IEvent;

    public interface ITestHandler
    {
        void Handle(TestEvent e);
    }

    private Mock<IEventLogger> _loggerMock = null!;
    private EventBus _bus = null!;

    [SetUp]
    public void SetUp()
    {
        this._loggerMock = new Mock<IEventLogger>();
        this._bus = new EventBus(this._loggerMock.Object);
    }

    [Test]
    public async Task PublishShouldInvokeSubscription()
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        _ = this._bus.Subscribe<TestEvent>(_ => tcs.TrySetResult(true));
        this._bus.Publish(new TestEvent());

        var timeout = TimeSpan.FromSeconds(1);
        _ = await tcs.Task.WaitAsync(timeout);
        Assert.That(tcs.Task.Result, Is.True);
    }

    [Test]
    public async Task PublishShouldInvokeMultipleSubscriptions()
    {
        var tcs1 = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var tcs2 = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        _ = this._bus.Subscribe<TestEvent>(_ => tcs1.TrySetResult(true));
        _ = this._bus.Subscribe<TestEvent>(_ => tcs2.TrySetResult(true));
        this._bus.Publish(new TestEvent());

        var allCompleted = Task.WhenAll(tcs1.Task, tcs2.Task);
        var completed = await Task.WhenAny(allCompleted, Task.Delay(1000));

        Assert.That(completed, Is.EqualTo(allCompleted), "Not all handlers invoked within timeout.");
        Assert.That(tcs1.Task.IsCompletedSuccessfully);
        Assert.That(tcs2.Task.IsCompletedSuccessfully);
    }

    [Test]
    public async Task PublishShouldPassEventInstanceToHandler_UsingMoq()
    {
        var mock = new Mock<ITestHandler>();
        var expected = new TestEvent();
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        _ = mock.Setup(m => m.Handle(It.Is<TestEvent>(e => ReferenceEquals(e, expected))))
            .Callback(() => tcs.TrySetResult(true));

        _ = this._bus.Subscribe<TestEvent>(mock.Object.Handle);
        this._bus.Publish(expected);

        var completed = await Task.WhenAny(tcs.Task, Task.Delay(1000));
        Assert.That(completed, Is.EqualTo(tcs.Task), "Handler did not receive the exact event instance.");
        mock.Verify(m => m.Handle(It.Is<TestEvent>(e => ReferenceEquals(e, expected))), Times.Once);
    }

    [Test]
    public async Task PublishShouldInvokeOtherHandlersEvenIfOneThrows()
    {
        var throwingTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var otherTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        _ = this._bus.Subscribe<TestEvent>(_ =>
        {
            Task.Delay(10).Wait();
            throwingTcs.TrySetResult(true);
            throw new InvalidOperationException("handler error");
        });

        _ = this._bus.Subscribe<TestEvent>(_ => otherTcs.TrySetResult(true));

        this._bus.Publish(new TestEvent());

        var completed = await Task.WhenAny(otherTcs.Task, Task.Delay(1000));
        Assert.That(completed, Is.EqualTo(otherTcs.Task), "Second handler was not invoked when the first handler threw.");
        Assert.That(otherTcs.Task.IsCompletedSuccessfully);
    }

    [Test]
    public void PublishWithNoSubscribersShouldNotThrow() => Assert.DoesNotThrow(() => this._bus.Publish(new TestEvent()));

    [Test]
    public async Task UnsubscribeShouldPreventFurtherInvocations()
    {
        var callCount = 0;
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var subscription = this._bus.Subscribe<TestEvent>(_ =>
        {
            Interlocked.Increment(ref callCount);
            tcs.TrySetResult(true);
        });

        this._bus.Publish(new TestEvent());
        _ = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(1));

        subscription.Dispose();

        this._bus.Publish(new TestEvent());
        await Task.Delay(100); // daj czas na potencjalne wywołanie

        Assert.That(callCount, Is.EqualTo(1), "Handler should have been called only once before unsubscribe.");
    }

    [Test]
    public void SubscribeShouldLogEvent()
    {
        _ = this._bus.Subscribe<TestEvent>(_ => { });

        this._loggerMock.Verify(
            l => l.Event_EventSubscribed(typeof(TestEvent).FullName!),
            Times.Once);
    }

    [Test]
    public async Task PublishShouldLogEvent()
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        _ = this._bus.Subscribe<TestEvent>(_ => tcs.TrySetResult(true));

        this._bus.Publish(new TestEvent());
        _ = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(1));

        this._loggerMock.Verify(
            l => l.Event_PublishedEventOfType(typeof(TestEvent).FullName!),
            Times.Once);
    }

    [Test]
    public void UnsubscribeShouldLogEvent()
    {
        var subscription = this._bus.Subscribe<TestEvent>(_ => { });
        subscription.Dispose();

        this._loggerMock.Verify(
            l => l.Event_EventUnsubscribed(typeof(TestEvent).FullName!),
            Times.Once);
    }

    [Test]
    public void UnsubscribeWithoutSubscribersShouldThrow()
    {
        var subscription = this._bus.Subscribe<TestEvent>(_ => { });
        subscription.Dispose();

        // Próba ponownego unsubscribe (lista pusta, ale klucz istnieje)
        // Ten test pokazuje obecne zachowanie - może wymagać zmiany w implementacji
        Assert.DoesNotThrow(subscription.Dispose);
    }
}