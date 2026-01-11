using System.Numerics;
using System.Text;

namespace TileStyle.Models;

public partial class Window
{
    private Rectangle _rect;

    public IntPtr Handle { get; init; }

    public string Title { get; init; }

    public Vector2 Position
    {
        get;
        set
        {
            field = value;
            _rect.X = (int)field.X;
            _rect.Y = (int)field.Y;
            ReRenderWindow();
        }
    }

    public Vector2 Size
    {
        get;
        set
        {
            field = value;
            _rect.Width = (int)field.X;
            _rect.Height = (int)field.Y;
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
            rect.Top,
            rect.Left,
            rect.Right - rect.Left,
            rect.Top - rect.Bottom
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
            SWP_NOZORDER | SWP_NOACTIVATE
        );
    }
}