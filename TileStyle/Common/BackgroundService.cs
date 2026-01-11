using System.Runtime.InteropServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TileStyle.Common;

public abstract class BackgroundService : IHostedService
{
    private Thread? _messageLoopThread;
    private CancellationTokenSource _cancellationTokenSource = new();

    protected abstract Task Initialize(CancellationToken cancellationToken);
    protected abstract Task Dispose(CancellationToken cancellationToken);

    protected Task UpdateLoop(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }


    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new();

        Initialize(_cancellationTokenSource.Token);
        InitializeMessageLoopThread(_cancellationTokenSource.Token);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        ShutdownThread();
        Dispose(cancellationToken);
        return Task.CompletedTask;
    }


    private void InitializeMessageLoopThread(CancellationToken cancellationToken)
    {
        _messageLoopThread = new Thread(async void () => await MessageLoopWorker(cancellationToken))
        {
            IsBackground = true
        };

        _messageLoopThread.SetApartmentState(ApartmentState.STA);
        _messageLoopThread.Start();
    }

    private async Task MessageLoopWorker(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await UpdateLoop(cancellationToken);
                Thread.Sleep(100);
            }
        }
        catch (Exception ex)
        {
        }
        finally
        {
            await Dispose(cancellationToken);
        }
    }

    private void ShutdownThread()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();

        if (_messageLoopThread != null && _messageLoopThread.IsAlive)
        {
            PostThreadMessage((uint)_messageLoopThread.ManagedThreadId, 0x0012, IntPtr.Zero, IntPtr.Zero); // WM_QUIT

            if (_messageLoopThread.Join(TimeSpan.FromSeconds(5)))
            {
                return;
            }

            _messageLoopThread.Interrupt();

            if (_messageLoopThread.Join(TimeSpan.FromSeconds(1)))
            {
                return;
            }

            _messageLoopThread = null;
        }
    }

    [DllImport("user32.dll")]
    private extern static bool PostThreadMessage(uint threadId, uint msg, IntPtr wParam, IntPtr lParam);
}