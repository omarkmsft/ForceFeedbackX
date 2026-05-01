using ForceFeedbackX.FFB;
using ForceFeedbackX.Physics;
using ForceFeedbackX.Profiles;
using ForceFeedbackX.SimConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace ForceFeedbackX;

public partial class App : Application
{
    public static IHost AppHost { get; private set; } = null!;

    private async void App_OnStartup(object sender, StartupEventArgs e)
    {
        await OnStartupAsync(e);
    }

    private async Task OnStartupAsync(StartupEventArgs e)
    {
        base.OnStartup(e);

        AppHost = Host.CreateDefaultBuilder()
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddDebug();
                logging.SetMinimumLevel(LogLevel.Debug);
            })
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton<SimConnectClient>();
                services.AddSingleton<FfbEngine>();
                services.AddSingleton<ForceCalculator>();
                services.AddSingleton<ProfileManager>();
                services.AddSingleton<MainWindow>();
            })
            .Build();

        await AppHost.StartAsync();

        var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await AppHost.StopAsync();
        AppHost.Dispose();
        base.OnExit(e);
    }
}
