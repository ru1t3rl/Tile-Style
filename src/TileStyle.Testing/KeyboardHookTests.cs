using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TileStyle.Keyboard;
using TileStyle.Windows;

namespace TileStyle.Testing;

public class KeyboardHookTests
{
    private ILogger<KeyboardHook> _logger = null!;
    private HiddenWindow _hiddenWindow = null!;
    private KeyboardHook _hook = null!;

    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<KeyboardHook>>();
        _hiddenWindow = new HiddenWindow();
        _hook = new KeyboardHook(_logger, _hiddenWindow);
    }

    [TearDown]
    public void TearDown()
    {
        _hook.Dispose();
        _hiddenWindow.Dispose();
    }

    [Test]
    public void Constructor_InitializesSuccessfully()
    {
        Assert.That(_hook, Is.Not.Null);
    }

    [Test]
    public void RegisteredHotKeys_InitiallyEmpty()
    {
        Assert.That(_hook.RegisteredHotKeys, Is.Empty);
    }

    [Test]
    public void RegisterHotKey_ReturnsValidHookId()
    {
        var hookId = _hook.RegisterHotKey(Keys.A);

        Assert.That(hookId, Is.GreaterThan(0));
    }

    [Test]
    public void RegisterHotKey_AddsToRegisteredHotKeys()
    {
        _hook.RegisterHotKey(Keys.A, ModifierKeys.Control);

        Assert.That(_hook.RegisteredHotKeys, Has.Count.EqualTo(1));
    }

    [Test]
    public void RegisterHotKey_MultipleKeys_ReturnsUniqueIds()
    {
        var hookId1 = _hook.RegisterHotKey(Keys.A, ModifierKeys.Control);
        var hookId2 = _hook.RegisterHotKey(Keys.B, ModifierKeys.Control);

        Assert.That(hookId1, Is.Not.EqualTo(hookId2));
    }

    [Test]
    public void RegisterHotKey_WithNoModifier_RegistersSuccessfully()
    {
        var hookId = _hook.RegisterHotKey(Keys.F1);

        Assert.That(hookId, Is.GreaterThan(0));
        Assert.That(_hook.RegisteredHotKeys, Has.Count.EqualTo(1));
    }

    [Test]
    public void RegisterHotKey_WithMultipleModifiers_RegistersSuccessfully()
    {
        var hookId = _hook.RegisterHotKey(Keys.A, ModifierKeys.Control | ModifierKeys.Shift);

        Assert.That(hookId, Is.GreaterThan(0));
        Assert.That(_hook.RegisteredHotKeys, Has.Count.EqualTo(1));
    }

    [Test]
    public void RegisterHotKey_SameKeyCombination_CreatesMultipleEntries()
    {
        var hookId1 = _hook.RegisterHotKey(Keys.A, ModifierKeys.Control);
        var hookId2 = _hook.RegisterHotKey(Keys.A, ModifierKeys.Control);

        Assert.That(_hook.RegisteredHotKeys, Has.Count.EqualTo(1));
        Assert.That(hookId1, Is.Not.EqualTo(hookId2));
    }

    [Test]
    public void UnregisterHotKey_RemovesRegisteredHotKey()
    {
        var hookId = _hook.RegisterHotKey(Keys.A, ModifierKeys.Control);

        _hook.UnregisterHotKey(hookId);

        Assert.That(_hook.RegisteredHotKeys, Is.Empty);
    }

    [Test]
    public void UnregisterHotKey_WithInvalidId_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => _hook.UnregisterHotKey(999));
    }

    [Test]
    public void UnregisterHotKey_MultipleKeys_RemovesOnlySpecified()
    {
        var hookIdToRemove = _hook.RegisterHotKey(Keys.A, ModifierKeys.Control);
        _hook.RegisterHotKey(Keys.B, ModifierKeys.Control);

        _hook.UnregisterHotKey(hookIdToRemove);

        Assert.That(_hook.RegisteredHotKeys, Has.Count.EqualTo(1));
    }

    [Test]
    public void UnregisterAllHotKeys_RemovesAllRegisteredHotKeys()
    {
        _hook.RegisterHotKey(Keys.A, ModifierKeys.Control);
        _hook.RegisterHotKey(Keys.B, ModifierKeys.Control);
        _hook.RegisterHotKey(Keys.C, ModifierKeys.Alt);

        _hook.UnregisterAllHotKeys();

        Assert.That(_hook.RegisteredHotKeys, Is.Empty);
    }

    [Test]
    public void UnregisterAllHotKeys_WhenEmpty_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => _hook.UnregisterAllHotKeys());
    }

    [Test]
    public void RegisteredHotKeys_ReturnsDistinctHotKeys()
    {
        _hook.RegisterHotKey(Keys.A, ModifierKeys.Control);
        _hook.RegisterHotKey(Keys.A, ModifierKeys.Control);
        _hook.RegisterHotKey(Keys.B, ModifierKeys.Control);

        var hotKeys = _hook.RegisteredHotKeys;

        Assert.That(hotKeys, Has.Count.EqualTo(2));
    }

    [Test]
    public void RegisterHotKey_WithWinModifier_RegistersSuccessfully()
    {
        var hookId = _hook.RegisterHotKey(Keys.D, ModifierKeys.Win);

        Assert.That(hookId, Is.GreaterThan(0));
        Assert.That(_hook.RegisteredHotKeys, Has.Count.EqualTo(1));
    }

    [Test]
    public void RegisterHotKey_WithAllModifiers_RegistersSuccessfully()
    {
        var allModifiers = ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Shift | ModifierKeys.Win;
        var hookId = _hook.RegisterHotKey(Keys.A, allModifiers);

        Assert.That(hookId, Is.GreaterThan(0));
        Assert.That(_hook.RegisteredHotKeys, Has.Count.EqualTo(1));
    }

    [Test]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        Assert.DoesNotThrow(() =>
        {
            _hook.Dispose();
            _hook.Dispose();
        });
    }

    [Test]
    public void Dispose_AfterRegisteringHotKeys_DoesNotThrow()
    {
        _hook.RegisterHotKey(Keys.A, ModifierKeys.Control);
        _hook.RegisterHotKey(Keys.B, ModifierKeys.Alt);

        Assert.DoesNotThrow(() => _hook.Dispose());
    }

    [Test]
    public void RegisterHotKey_AfterUnregisteringAll_StartsFromNewId()
    {
        var hookId1 = _hook.RegisterHotKey(Keys.A);
        _hook.UnregisterAllHotKeys();
        var hookId2 = _hook.RegisterHotKey(Keys.B);

        Assert.That(hookId2, Is.GreaterThan(hookId1));
    }

    [Test]
    public void KeyPressed_EventCanBeSubscribed()
    {
        bool eventRaised = false;
        _hook.KeyPressed += (_, _) => eventRaised = true;

        Assert.That(eventRaised, Is.False);
    }

    [Test]
    public void RegisterHotKey_WithFunctionKeys_RegistersSuccessfully()
    {
        _hook.RegisterHotKey(Keys.F1);
        _hook.RegisterHotKey(Keys.F12);

        Assert.That(_hook.RegisteredHotKeys, Has.Count.EqualTo(2));
    }

    [Test]
    public void RegisterHotKey_WithNumericKeys_RegistersSuccessfully()
    {
        _hook.RegisterHotKey(Keys.D1, ModifierKeys.Control);
        _hook.RegisterHotKey(Keys.D9, ModifierKeys.Control);

        Assert.That(_hook.RegisteredHotKeys, Has.Count.EqualTo(2));
    }

    [Test]
    public void RegisterHotKey_WithSpecialKeys_RegistersSuccessfully()
    {
        _hook.RegisterHotKey(Keys.Space, ModifierKeys.Control);
        _hook.RegisterHotKey(Keys.Enter, ModifierKeys.Alt);
        _hook.RegisterHotKey(Keys.Escape);

        Assert.That(_hook.RegisteredHotKeys, Has.Count.EqualTo(3));
    }

    [Test]
    public void UnregisterHotKey_SameIdTwice_DoesNotThrow()
    {
        var hookId = _hook.RegisterHotKey(Keys.A);

        _hook.UnregisterHotKey(hookId);
        Assert.DoesNotThrow(() => _hook.UnregisterHotKey(hookId));
    }

    [Test]
    public void RegisteredHotKeys_AfterRegisteringAndUnregistering_ReturnsCorrectCount()
    {
        var hookIdToRemove = _hook.RegisterHotKey(Keys.A);
        _hook.RegisterHotKey(Keys.B);
        _hook.RegisterHotKey(Keys.C);

        _hook.UnregisterHotKey(hookIdToRemove);

        Assert.That(_hook.RegisteredHotKeys, Has.Count.EqualTo(2));
    }
}