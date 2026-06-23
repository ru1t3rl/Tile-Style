using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TileStyle.Models;
using TileStyle.Windows;

namespace TileStyle.Testing;

public class WindowManagerTests
{
    private ILogger<WindowManager> _logger = null!;
    private WindowEventHook _windowEventHook = null!;
    private VirtualDesktopHelper _virtualDesktop = null!;
    private WindowManager _windowManager = null!;
    private WindowStyleChanger _styleChanger = null!;
    
    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<WindowManager>>();
        _windowEventHook = Substitute.For<WindowEventHook>(Substitute.For<ILogger<WindowEventHook>>());
        _virtualDesktop = Substitute.For<VirtualDesktopHelper>(Substitute.For<ILogger<VirtualDesktopHelper>>());
        _styleChanger = Substitute.For<WindowStyleChanger>();
        
        _windowManager = new WindowManager(_windowEventHook, _virtualDesktop, _logger, _styleChanger);
    }

    [TearDown]
    public void TearDown()
    {
        _windowEventHook.Dispose();
        _windowManager.Dispose();
    }

    [Test]
    public void Constructor_InitializesWindowsListAsEmpty()
    {
        Assert.That(_windowManager.ScreenGroupedWindows, Is.Empty);
    }

    [Test]
    public void Constructor_InitializesZonesListAsEmpty()
    {
        Assert.That(_windowManager.Zones, Is.Empty);
    }

    [Test]
    public void ToggleTiling_TogglesEnabledState()
    {
        _windowManager.ToggleTiling();
        _windowManager.ToggleTiling();

        Assert.Pass();
    }

    [Test]
    public void CloseWindow_RemovesWindowFromList()
    {
        var testHandle = new IntPtr(12345);
        var testWindow = CreateMockWindow(testHandle);
        Screen screen = Screen.FromHandle(testWindow.Handle);
        _windowManager.ScreenGroupedWindows.Add(screen, [testWindow]);

        var args = new WindowEventArgs(testHandle);
        _windowManager.CloseWindowAsync(null, args);

        Assert.That(_windowManager.ScreenGroupedWindows[screen], Does.Not.Contain(testWindow));
    }

    [Test]
    public void CloseWindow_DoesNothingWhenWindowNotFound()
    {
        var nonExistentHandle = new IntPtr(99999);
        var args = new WindowEventArgs(nonExistentHandle);

        Assert.DoesNotThrow(() => _windowManager.CloseWindowAsync(null, args));
    }

    [Test]
    public void CloseWindow_RemovesWindowFromZone()
    {
        var testHandle = new IntPtr(12345);
        var testWindow = CreateMockWindow(testHandle);

        var zone = new Zone
        {
            DesktopId = Guid.NewGuid(),
            Area = new Rectangle { X = 0, Y = 0, Width = 100, Height = 100 }
        };

        Screen screen = Screen.FromHandle(testWindow.Handle);
        _windowManager.ScreenGroupedWindows.Add(screen, [testWindow]);
        _windowManager.Zones.Add(zone);
        zone.AddWindow(testWindow);

        var args = new WindowEventArgs(testHandle);
        _windowManager.CloseWindowAsync(null, args);

        Assert.That(zone.Windows, Does.Not.Contain(testWindow));
    }

    [Test]
    public void Dispose_DisposesWindowEventHook()
    {
        _windowManager.Dispose();

        _windowEventHook.Received(1).Dispose();
    }

    [Test]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        Assert.DoesNotThrow(() =>
        {
            _windowManager.Dispose();
            _windowManager.Dispose();
        });
    }

    [Test]
    public void MaxWindowsPerZone_IsSetToTwo()
    {
        Assert.That(WindowManager.MAX_WINDOWS_PER_ZONE, Is.EqualTo(2));
    }

    private static Window CreateMockWindow(IntPtr handle)
    {
        var windowType = typeof(Window);
        var window = (Window)RuntimeHelpers.GetUninitializedObject(windowType);

        var handleField = windowType.GetField("<Handle>k__BackingField",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        handleField?.SetValue(window, handle);

        return window;
    }
}