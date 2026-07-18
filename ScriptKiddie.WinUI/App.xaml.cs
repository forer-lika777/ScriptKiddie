using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.UI.Xaml;
#if DEBUG
using ScriptKiddie.WinUI.Mocks;
#endif
using ScriptKiddie.WinUI.Services;
using ScriptKiddie.WinUI.ViewModels;
using System;
using System.Runtime.InteropServices;

namespace ScriptKiddie.WinUI;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    private Window? window;

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
#if DEBUG
        AllocConsole();
#endif

        Services = ConfigureServices();

#if DEBUG
        var logger = Services.GetRequiredService<ILogger<App>>();
        logger.LogInformation("═══════════════════════════════════════════");
        logger.LogInformation("  调试控制台已启动");
        logger.LogInformation($"  {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        logger.LogInformation("═══════════════════════════════════════════");
#endif

        this.InitializeComponent();
    }

#if DEBUG
    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool AllocConsole();
#endif

    /// <summary>
    /// Gets the current <see cref="App"/> instance in use
    /// </summary>
    public new static App Current => (App)Application.Current;

    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
    /// </summary>
    public IServiceProvider Services { get; }

    /// <summary>
    /// Configures the services for the application.
    /// </summary>
    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        AddServices(services);
        AddViewModels(services);
        AddViews(services);
        AddLogger(services);

        return services.BuildServiceProvider();
    }

    private static void AddViewModels(ServiceCollection services)
    {
        services.AddSingleton<CourseListPageModel>();
        services.AddSingleton<MainWindowModel>();
        services.AddSingleton<LoginPageModel>();
        services.AddSingleton<AccountManagePageModel>();
        services.AddSingleton<SettingPageModel>();
    }

    private static void AddServices(ServiceCollection services)
    {
        services.AddSingleton<AppSettingsService>();

        services.AddSingleton<ICourseSelectService, CourseSelectService>();
        services.AddSingleton<ILoginService, MockLoginService>();

        services.AddSingleton<AccountManageService>();

        services.AddSingleton<HttpClientProvider>();
    }

    private static void AddViews(ServiceCollection services)
    {
        services.AddSingleton<MainWindow>();
    }

    private static void AddLogger(ServiceCollection services)
    {
        services.AddLogging(builder =>
        {
#if DEBUG
            builder.AddSimpleConsole(options =>
            {
                options.SingleLine = true;
                options.IncludeScopes = false;
                options.ColorBehavior = LoggerColorBehavior.Enabled;
            });
            builder.SetMinimumLevel(LogLevel.Debug);
#endif
        });
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        window = Current.Services.GetRequiredService<MainWindow>();
        window.Activate();
    }
}
