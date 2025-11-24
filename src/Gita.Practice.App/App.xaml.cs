using System.Configuration;
using System.Data;
using System.Windows;
using Gita.Practice.App.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Gita.Practice.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{   
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Register framework services
                services.AddLogging();

                // Register application services, viewmodels and windows here.
                // Example (uncomment and replace with real types):
                // services.AddSingleton<IMyService, MyService>();
                // services.AddTransient<MainWindow>();
                //
                // Keep registrations minimal here; prefer to move complex registrations
                // into separate extension methods or modules for clarity.

                ConfigureServices(services);
            })
            .Build();

        await _host.StartAsync().ConfigureAwait(false);

        // Try to resolve a Window from DI
        var serviceProvider = _host.Services;
        Window? mainWindow = serviceProvider.GetService<Window>();

        if (mainWindow == null)
        {
            // Try to find a "MainWindow" type via reflection in this assembly
            var asm = typeof(App).Assembly;
            var mwType = asm.GetType("Gita.Practice.App.MainWindow")
                         ?? asm.GetTypes().FirstOrDefault(t => typeof(Window).IsAssignableFrom(t) && t.Name.EndsWith("MainWindow", StringComparison.OrdinalIgnoreCase));

            if (mwType != null)
            {
                try
                {
                    mainWindow = (Window)ActivatorUtilities.CreateInstance(serviceProvider, mwType);
                }
                catch (Exception ex)
                {
                    var logger = serviceProvider.GetService<ILogger<App>>();
                    logger?.LogError(ex, "Failed to create MainWindow via DI/ActivatorUtilities.");
                }
            }
        }

        if (mainWindow != null)
        {
            // Set Application.MainWindow and show
            MainWindow = mainWindow;
            mainWindow.DataContext = new MainViewModel();
            mainWindow.Show();
        }
        else
        {
            var logger = serviceProvider.GetService<ILogger<App>>();
            logger?.LogWarning("No MainWindow was resolved or discovered. Application started without a main window.");
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);

        if (_host != null)
        {
            try
            {
                await _host.StopAsync().ConfigureAwait(false);
            }
            catch
            {
                // ignore exceptions during shutdown
            }
            finally
            {
                _host.Dispose();
                _host = null;
            }
        }
    }

    // Extend this method to register your services. Keeps host-building code clean.
    private void ConfigureServices(IServiceCollection services)
    {
        // Put DI registrations here. Examples:
        services.AddTransient<IDataRepository, DataRepository>();
        services.AddTransient<MainViewModel>();
    }

    private void Application_Startup(object sender, StartupEventArgs e)
    {

    }
}

