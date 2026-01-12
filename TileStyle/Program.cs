using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using TileStyle.Consumers;
using TileStyle.Keyboard;
using TileStyle.Windows;

namespace TileStyle;

static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        IHost host = Host.CreateDefaultBuilder()
            .ConfigureServices(ConfigureServices)
            .Build();

        host.StartAsync();

        WindowManagerContext context = host.Services.GetRequiredService<WindowManagerContext>();
        WindowManager windowManager = host.Services.GetRequiredService<WindowManager>();
        windowManager.InitializeContext();

        Application.Run(context);

        host.StopAsync().Wait();
    }

    static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services.AddSingleton<WindowManagerContext>();
        services.AddSingleton<WindowManager>();
        services.AddSingleton<WindowEventHook>();

        services.AddSingleton<VirtualDesktopHelper>();
        services.AddSingleton<HiddenWindow>();

        services.Scan(scan => scan
            .FromAssembliesOf(typeof(NamespaceAnchor))
            .AddClasses(c => c.AssignableTo<IKeyConsumer>())
            .AsImplementedInterfaces()
        );

        services.AddSerilog(config =>
        {
            config.WriteTo.Console();
            config.MinimumLevel.Information();
        });

        services.AddHostedService<KeyboardEventService>();
        services.AddScoped<IKeyboardHook, LowLevelKeyboardHook>();
    }
}