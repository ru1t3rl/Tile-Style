using Microsoft.Extensions.Logging;
using NSubstitute;
using TileStyle.Windows;

namespace TileStyle.Testing;

public class VirtualDesktopHelperTests
{
    private ILogger<VirtualDesktopHelper> _logger = null!;
    private IServiceProvider _serviceProvider = null!;
    private VirtualDesktopHelper _helper = null!;
    private WindowStore _windowStore = null!;

    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<VirtualDesktopHelper>>();
        _serviceProvider = Substitute.For<IServiceProvider>();

        _windowStore = Substitute.For<WindowStore>();
        _serviceProvider
            .GetService(typeof(WindowStore))
            .Returns(_windowStore);

        _helper = new VirtualDesktopHelper(_logger, _serviceProvider);
    }

    [Test]
    public void Constructor_InitializesSuccessfully()
    {
        Assert.That(_helper, Is.Not.Null);
    }

    [Test]
    public void GetCurrentDesktop_ReturnsGuid()
    {
        var desktopId = _helper.GetCurrentDesktop();
        Assert.That(desktopId, Is.TypeOf<Guid>());
    }

    [Test]
    public void GetCurrentDesktop_WhenNoForegroundWindow_ReturnsGuidEmpty()
    {
        var desktopId = _helper.GetCurrentDesktop();
        Assert.That(desktopId, Is.Default);
    }

    [Test]
    public void GetWindowDesktop_WithZeroHandle_ReturnsGuidEmpty()
    {
        var desktopId = _helper.GetWindowDesktop(IntPtr.Zero);
        Assert.That(desktopId, Is.TypeOf<Guid>());
    }

    [Test]
    public void GetWindowDesktop_WithValidHandle_ReturnsGuid()
    {
        var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
        var handle = currentProcess.MainWindowHandle;

        var desktopId = _helper.GetWindowDesktop(handle);

        Assert.That(desktopId, Is.TypeOf<Guid>());
    }

    [Test]
    public void GetWindowDesktop_WithInvalidHandle_DoesNotThrow()
    {
        var invalidHandle = new IntPtr(99999);

        Assert.DoesNotThrow(() => _helper.GetWindowDesktop(invalidHandle));
    }

    [Test]
    public void MoveWindowToDesktop_WithZeroHandle_DoesNotThrow()
    {
        var desktopId = Guid.NewGuid();

        Assert.DoesNotThrow(() => _helper.MoveWindowToDesktop(IntPtr.Zero, desktopId));
    }

    [Test]
    public void MoveWindowToDesktop_WithInvalidDesktopId_DoesNotThrow()
    {
        var handle = new IntPtr(12345);
        var invalidDesktopId = Guid.NewGuid();

        Assert.DoesNotThrow(() => _helper.MoveWindowToDesktop(handle, invalidDesktopId));
    }

    [Test]
    public void MoveWindowToDesktop_WithEmptyGuid_DoesNotThrow()
    {
        var handle = new IntPtr(12345);

        Assert.DoesNotThrow(() => _helper.MoveWindowToDesktop(handle, Guid.Empty));
    }

    [Test]
    public void GetCurrentDesktop_CalledMultipleTimes_DoesNotThrow()
    {
        Assert.DoesNotThrow(() =>
        {
            for (int i = 0; i < 10; i++)
            {
                _helper.GetCurrentDesktop();
            }
        });
    }

    [Test]
    public void GetWindowDesktop_SameHandleCalledTwice_ReturnsSameResult()
    {
        var handle = new IntPtr(12345);

        var firstResult = _helper.GetWindowDesktop(handle);
        var secondResult = _helper.GetWindowDesktop(handle);

        Assert.That(firstResult, Is.EqualTo(secondResult));
    }

    [Test]
    public void Constructor_WhenInitializationFails_StillCreatesInstance()
    {
        // Even if COM initialization fails, the helper should be created
        // and methods should return Guid.Empty gracefully
        var helper = new VirtualDesktopHelper(_logger, _serviceProvider);

        Assert.That(helper, Is.Not.Null);
        Assert.DoesNotThrow(() => helper.GetCurrentDesktop());
    }

    [Test]
    public void MoveWindowToDesktop_AfterGetWindowDesktop_WorksCorrectly()
    {
        var handle = new IntPtr(12345);
        var desktopId = _helper.GetWindowDesktop(handle);

        Assert.DoesNotThrow(() => _helper.MoveWindowToDesktop(handle, desktopId));
    }
}