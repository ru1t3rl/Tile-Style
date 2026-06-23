using Microsoft.Extensions.Logging;
using NSubstitute;
using TileStyle.Windows;

namespace TileStyle.Testing;

public class WindowEventHookTests
{
    private ILogger<WindowEventHook> _logger = null!;

    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<WindowEventHook>>();
    }

    [Test]
    public void Constructor_InitializesWithoutErrors()
    {
        Assert.DoesNotThrow(() =>
        {
            using var hook = new WindowEventHook(_logger);
        });
    }

    [Test]
    public void WindowCreated_EventIsRaisedWithCorrectHandle()
    {
        using var hook = new WindowEventHook(_logger);
        var expectedHandle = new IntPtr(12345);
        WindowEventArgs? capturedArgs = null;

        hook.WindowCreated += (_, args) => Task.FromResult(capturedArgs = args);
        InvokeWindowEventProcessor(hook, 0x8000, expectedHandle);

        Assert.That(capturedArgs, Is.Not.Null);
        Assert.That(capturedArgs!.WindowHandle, Is.EqualTo(expectedHandle));
    }

    [Test]
    public void WindowDestroyed_EventIsRaisedWithCorrectHandle()
    {
        using var hook = new WindowEventHook(_logger);
        var expectedHandle = new IntPtr(54321);
        WindowEventArgs? capturedArgs = null;

        hook.WindowDestroyed += (_, args) => Task.FromResult(capturedArgs = args);
        InvokeWindowEventProcessor(hook, 0x8001, expectedHandle);

        Assert.That(capturedArgs, Is.Not.Null);
        Assert.That(capturedArgs!.WindowHandle, Is.EqualTo(expectedHandle));
    }

    [Test]
    public void WindowShown_EventIsRaisedWithCorrectHandle()
    {
        using var hook = new WindowEventHook(_logger);
        var expectedHandle = new IntPtr(99999);
        WindowEventArgs? capturedArgs = null;

        hook.WindowShown += (_, args) => Task.FromResult(capturedArgs = args);

        InvokeWindowEventProcessor(hook, 0x8002, expectedHandle);

        Assert.That(capturedArgs, Is.Not.Null);
        Assert.That(capturedArgs!.WindowHandle, Is.EqualTo(expectedHandle));
    }

    [Test]
    public void WindowMinimized_EventIsRaisedForHideEvent()
    {
        using var hook = new WindowEventHook(_logger);
        var expectedHandle = new IntPtr(11111);
        WindowEventArgs? capturedArgs = null;

        hook.WindowMinimized += (_, args) => Task.FromResult(capturedArgs = args);
        InvokeWindowEventProcessor(hook, 0x8003, expectedHandle);

        Assert.That(capturedArgs, Is.Not.Null);
        Assert.That(capturedArgs!.WindowHandle, Is.EqualTo(expectedHandle));
    }

    [Test]
    public void WindowMinimized_EventIsRaisedForMinimizeStartEvent()
    {
        using var hook = new WindowEventHook(_logger);
        var expectedHandle = new IntPtr(22222);
        WindowEventArgs? capturedArgs = null;

        hook.WindowMinimized += (_, args) => Task.FromResult(capturedArgs = args);
        InvokeWindowEventProcessor(hook, 0x0016, expectedHandle);

        Assert.That(capturedArgs, Is.Not.Null);
        Assert.That(capturedArgs!.WindowHandle, Is.EqualTo(expectedHandle));
    }

    [Test]
    public void WindowRestored_EventIsRaisedWithCorrectHandle()
    {
        using var hook = new WindowEventHook(_logger);
        var expectedHandle = new IntPtr(33333);
        WindowEventArgs? capturedArgs = null;

        hook.WindowRestored += (_, args) => Task.FromResult(capturedArgs = args);
        InvokeWindowEventProcessor(hook, 0x0017, expectedHandle);

        Assert.That(capturedArgs, Is.Not.Null);
        Assert.That(capturedArgs!.WindowHandle, Is.EqualTo(expectedHandle));
    }

    [Test]
    public void WindowEventProcessor_IgnoresEventsWithNonZeroIdObject()
    {
        using var hook = new WindowEventHook(_logger);
        var eventRaised = false;

        hook.WindowCreated += (_, _) => Task.FromResult(eventRaised = true);
        InvokeWindowEventProcessor(hook, 0x8000, new IntPtr(12345), idObject: 1);

        Assert.That(eventRaised, Is.False);
    }

    [Test]
    public void WindowEventProcessor_IgnoresEventsWithNonZeroIdChild()
    {
        using var hook = new WindowEventHook(_logger);
        var eventRaised = false;

        hook.WindowCreated += (_, _) => Task.FromResult(eventRaised = true);
        InvokeWindowEventProcessor(hook, 0x8000, new IntPtr(12345), idChild: 1);

        Assert.That(eventRaised, Is.False);
    }

    [Test]
    public void WindowEventProcessor_IgnoresEventsWithZeroHandle()
    {
        using var hook = new WindowEventHook(_logger);
        var eventRaised = false;

        hook.WindowCreated += (_, _) => Task.FromResult(eventRaised = true);
        InvokeWindowEventProcessor(hook, 0x8000, IntPtr.Zero);

        Assert.That(eventRaised, Is.False);
    }

    [Test]
    public void Dispose_UnhooksAllEventHooks()
    {
        var hook = new WindowEventHook(_logger);

        hook.Dispose();

        _logger.Received(3).Log(
            LogLevel.Debug,
            Arg.Any<EventId>(),
            Arg.Is<object>(s => s.ToString()!.Contains("Unhooked WinEvent Hook")),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>()
        );
    }

    [Test]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        var hook = new WindowEventHook(_logger);

        Assert.DoesNotThrow(() =>
        {
            hook.Dispose();
            hook.Dispose();
        });
    }

    [Test]
    public void MultipleEventHandlers_AllAreInvoked()
    {
        using var hook = new WindowEventHook(_logger);
        var handler1Called = false;
        var handler2Called = false;

        hook.WindowCreated += (_, _) => Task.FromResult(handler1Called = true);
        hook.WindowCreated += (_, _) => Task.FromResult(handler2Called = true);

        InvokeWindowEventProcessor(hook, 0x8000, new IntPtr(12345));

        Assert.That(handler1Called, Is.True);
        Assert.That(handler2Called, Is.True);
    }

    [Test]
    public void UnhandledEventType_LogsWarning()
    {
        using var hook = new WindowEventHook(_logger);

        InvokeWindowEventProcessor(hook, 0xFFFF, new IntPtr(12345));

        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(state => state.ToString()!.Contains("Unhandled event type")),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>()
        );
    }

    // Helper method to invoke the private WindowEventProcessor via reflection
    private static void InvokeWindowEventProcessor(
        WindowEventHook hook,
        uint eventType,
        IntPtr hwnd,
        int idObject = 0,
        int idChild = 0)
    {
        var method = typeof(WindowEventHook).GetMethod(
            "WindowEventProcessor",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method?.Invoke(hook, new object[]
        {
            IntPtr.Zero, // hWinEventHook
            eventType,
            hwnd,
            idObject,
            idChild,
            0u, // dwEventThread
            0u // dwmsEventTime
        });
    }
}