using System.Numerics;

namespace TileStyle.Models;

public class Zone
{
    public Guid DesktopId { get; init; }
    public Rectangle Area { get; init; }
    public LayoutMode Mode { get; private set; } = LayoutMode.Horizontal;

    private readonly List<Window> _windows = new();
    public IReadOnlyList<Window> Windows => _windows.AsReadOnly();

    private void UpdateZone()
    {
        Window[] windows = _windows
            .Where(w => !w.Floating && w.Visible)
            .ToArray();

        switch (Mode)
        {
            case LayoutMode.Horizontal:
                UpdateHorizontally(windows);
                break;
            case LayoutMode.Vertical:
                UpdateVertically(windows);
                break;
        }
    }

    private void UpdateHorizontally(Window[] windows)
    {
        int windowWidth = Area.Width / windows.Length;
        int windowHeight = Area.Height;

        for (int iWindow = 0; iWindow < windows.Length; iWindow++)
        {
            windows[iWindow].Position = new Vector2(
                Area.X + windowWidth * iWindow,
                Area.Y
            );

            windows[iWindow].Size = new Vector2(windowWidth, windowHeight);
        }
    }

    private void UpdateVertically(Window[] windows)
    {
        int windowWidth = Area.Width;
        int windowHeight = Area.Height / windows.Length;

        for (int iWindow = 0; iWindow < windows.Length; iWindow++)
        {
            windows[iWindow].Position = new Vector2(
                Area.X,
                Area.Y + windowHeight * iWindow
            );

            windows[iWindow].Size = new Vector2(windowWidth, windowHeight);
        }
    }

    public void SetLayoutMode(LayoutMode mode)
    {
        Mode = mode;
        UpdateZone();
    }

    public void AddWindowRange(Window[] windows)
    {
        _windows.AddRange(windows);
        UpdateZone();
    }

    public void AddWindow(Window window)
    {
        _windows.Add(window);
        UpdateZone();
    }

    public void RemoveWindow(IntPtr windowHandle)
    {
        _windows.RemoveAll(w => w.Handle == windowHandle);
        UpdateZone();
    }
    
    public bool IsEmpty => _windows.Count == 0;
}