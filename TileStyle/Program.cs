using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TileStyle.Consumers;
using TileStyle.Keyboard;
using TileStyle.Window;

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
        Application.Run(context);

        host.StopAsync().Wait();
    }

    static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services.AddSingleton<WindowManagerContext>();
        services.AddSingleton<WindowManager>();
        services.AddSingleton<VirtualDesktopHelper>();
        services.AddSingleton<WindowEventHook>();

        services.AddSingleton<HiddenWindow>();

        services.Scan(scan => scan
            .FromAssembliesOf(typeof(NamespaceAnchor))
            .AddClasses(c => c.AssignableTo<IKeyConsumer>())
            .AsImplementedInterfaces()
        );

        services.AddHostedService<KeyboardEventService>();

        services.AddScoped<KeyboardHook>();
    }
}