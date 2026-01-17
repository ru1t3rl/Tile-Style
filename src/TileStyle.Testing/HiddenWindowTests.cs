using System.Reflection;
using System.Windows.Forms;
using TileStyle.Keyboard;
using TileStyle.Windows;

namespace TileStyle.Testing;

public class HiddenWindowTests
{
    [Test]
    public void Constructor_CreatesWindowHandle()
    {
        using var window = new HiddenWindow();

        Assert.That(window.Handle, Is.Not.EqualTo(IntPtr.Zero));
    }

    [Test]
    public void HotKeyPressed_EventIsRaisedWhenHotKeyMessageReceived()
    {
        using var window = new HiddenWindow();
        HotKeyEventArgs? capturedArgs = null;
        var expectedHotKeyId = 42;

        window.HotKeyPressed += (_, args) => capturedArgs = args;

        SendHotKeyMessage(window, expectedHotKeyId);

        Assert.That(capturedArgs, Is.Not.Null);
        Assert.That(capturedArgs!.HotKeyHookId, Is.EqualTo(expectedHotKeyId));
    }

    [Test]
    public void HotKeyPressed_MultipleHandlersAreInvoked()
    {
        using var window = new HiddenWindow();
        var handler1Called = false;
        var handler2Called = false;
        var hotKeyId = 100;

        window.HotKeyPressed += (_, _) => handler1Called = true;
        window.HotKeyPressed += (_, _) => handler2Called = true;

        SendHotKeyMessage(window, hotKeyId);

        Assert.That(handler1Called, Is.True);
        Assert.That(handler2Called, Is.True);
    }

    [Test]
    public void HotKeyPressed_CorrectSenderIsProvided()
    {
        using var window = new HiddenWindow();
        object? capturedSender = null;

        window.HotKeyPressed += (sender, _) => capturedSender = sender;

        SendHotKeyMessage(window, 1);

        Assert.That(capturedSender, Is.SameAs(window));
    }

    [Test]
    public void HotKeyPressed_NotRaisedForOtherMessages()
    {
        using var window = new HiddenWindow();
        var eventRaised = false;

        window.HotKeyPressed += (_, _) => eventRaised = true;

        SendMessage(window, 0x0010, IntPtr.Zero);

        Assert.That(eventRaised, Is.False);
    }

    [Test]
    public void HotKeyPressed_HandlesMultipleHotKeyIds()
    {
        using var window = new HiddenWindow();
        var receivedIds = new List<int>();

        window.HotKeyPressed += (_, args) => receivedIds.Add(args.HotKeyHookId);

        SendHotKeyMessage(window, 1);
        SendHotKeyMessage(window, 2);
        SendHotKeyMessage(window, 99);

        Assert.That(receivedIds, Is.EqualTo(new[] { 1, 2, 99 }));
    }

    [Test]
    public void HotKeyPressed_NoHandlers_DoesNotThrow()
    {
        using var window = new HiddenWindow();

        Assert.DoesNotThrow(() => SendHotKeyMessage(window, 1));
    }

    [Test]
    public void Dispose_DestroysHandle()
    {
        var window = new HiddenWindow();
        var handleBeforeDispose = window.Handle;

        window.Dispose();

        Assert.That(handleBeforeDispose, Is.Not.EqualTo(IntPtr.Zero));
        Assert.That(window.Handle, Is.EqualTo(IntPtr.Zero));
    }

    [Test]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        var window = new HiddenWindow();

        Assert.DoesNotThrow(() =>
        {
            window.Dispose();
            window.Dispose();
        });
    }

    private static void SendHotKeyMessage(HiddenWindow window, int hotKeyId)
    {
        SendMessage(window, 0x0312, new IntPtr(hotKeyId));
    }

    private static void SendMessage(HiddenWindow window, int msg, IntPtr wParam)
    {
        var wndProcMethod = typeof(NativeWindow).GetMethod(
            "WndProc",
            BindingFlags.NonPublic | BindingFlags.Instance);

        var message = Message.Create(window.Handle, msg, wParam, IntPtr.Zero);
        
        wndProcMethod?.Invoke(window, [message]);
    }
}