using Moq;

namespace Watcher.Runner.Tests.Unit;

[TestFixture]
public class EventBusTests
{
    public record TestEvent : IEvent;

    public interface ITestHandler
    {
        void Handle(TestEvent e);
    }

    [Test]
    public async Task PublishShouldInvokeSubscription()
    {
        var bus = new EventBus();
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        bus.Subscribe<TestEvent>(_ => tcs.TrySetResult(true));
        bus.Publish(new TestEvent());

        var timeout = TimeSpan.FromSeconds(1);
        await tcs.Task.WaitAsync(timeout);
        Assert.That(tcs.Task.Result, Is.True);
    }

    [Test]
    public async Task PublishShouldInvokeMultipleSubscriptions()
    {
        var bus = new EventBus();
        var tcs1 = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var tcs2 = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        bus.Subscribe<TestEvent>(_ => tcs1.TrySetResult(true));
        bus.Subscribe<TestEvent>(_ => tcs2.TrySetResult(true));
        bus.Publish(new TestEvent());

        var completed = await Task.WhenAny(Task.WhenAll(tcs1.Task, tcs2.Task), Task.Delay(1000));
        Assert.That(completed, Is.Not.EqualTo(Task.Delay(1000)), "Not all handlers invoked within timeout.");
        Assert.That(tcs1.Task.IsCompletedSuccessfully);
        Assert.That(tcs2.Task.IsCompletedSuccessfully);
    }

    [Test]
    public async Task PublishShouldPassEventInstanceToHandler_UsingMoq()
    {
        var bus = new EventBus();
        var mock = new Mock<ITestHandler>();
        var expected = new TestEvent();
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        mock.Setup(m => m.Handle(It.Is<TestEvent>(e => ReferenceEquals(e, expected))))
            .Callback(() => tcs.TrySetResult(true));

        bus.Subscribe<TestEvent>(e => mock.Object.Handle(e));
        bus.Publish(expected);

        var completed = await Task.WhenAny(tcs.Task, Task.Delay(1000));
        Assert.That(completed, Is.EqualTo(tcs.Task), "Handler did not receive the exact event instance.");
        mock.Verify(m => m.Handle(It.Is<TestEvent>(e => ReferenceEquals(e, expected))), Times.Once);
    }

    [Test]
    public async Task PublishShouldInvokeOtherHandlersEvenIfOneThrows()
    {
        var bus = new EventBus();
        var throwingTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var otherTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        bus.Subscribe<TestEvent>(_ =>
        {
            Task.Delay(10).Wait();
            throwingTcs.TrySetResult(true);
            throw new InvalidOperationException("handler error");
        });

        bus.Subscribe<TestEvent>(_ => otherTcs.TrySetResult(true));

        bus.Publish(new TestEvent());

        var completed = await Task.WhenAny(otherTcs.Task, Task.Delay(1000));
        Assert.That(completed, Is.EqualTo(otherTcs.Task), "Second handler was not invoked when the first handler threw.");
        Assert.That(otherTcs.Task.IsCompletedSuccessfully);
    }

    [Test]
    public void PublishWithNoSubscribersShouldNotThrow()
    {
        var bus = new EventBus();
        Assert.DoesNotThrow(() => bus.Publish(new TestEvent()));
    }
}