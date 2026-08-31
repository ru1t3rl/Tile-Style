using System.Numerics;
using System.Text;

namespace TileStyle.Models;

public partial class Window
{
    private Rectangle _rect;

    public IntPtr Handle { get; init; }

    public string Title { get; init; }

    public int PositionOffset { get; set; } = 0;

    public Vector2 Position
    {
        get => new(_rect.X, _rect.Y);
        set
        {
            _rect.X = (int)value.X;
            _rect.Y = (int)value.Y;
            ReRenderWindow();
        }
    }

    public Vector2 Size
    {
        get => new(_rect.Width, _rect.Height);
        set
        {
            _rect.Width = (int)value.X;
            _rect.Height = (int)value.Y;
            ReRenderWindow();
        }
    }

    public bool Floating { get; set; } = false;
    public bool Visible => IsWindowVisible(Handle);
    public bool Minimized => IsIconic(Handle);

    public Window(IntPtr handle)
    {
        Handle = handle;
        Title = GetWindowTitle(Handle);

        GetWindowRect(handle, out var rect);
        _rect = new Rectangle(
            rect.Left,
            rect.Top,
            rect.Right - rect.Left,
            rect.Bottom - rect.Top
        );
    }

    private string GetWindowTitle(IntPtr hwnd)
    {
        int length = GetWindowTextLength(hwnd);
        if (length == 0) return string.Empty;

        StringBuilder sb = new StringBuilder(length + 1);
        GetWindowText(hwnd, sb, sb.Capacity);
        return sb.ToString();
    }

    private void ReRenderWindow()
    {
        if (Floating)
        {
            return;
        }

        SetWindowPos(
            Handle,
            IntPtr.Zero,
            _rect.X,
            _rect.Y,
            _rect.Width,
            _rect.Height,
            SWP_ASYNCWINDOWPOS | SWP_NOZORDER | SWP_NOACTIVATE
        );
    }
}