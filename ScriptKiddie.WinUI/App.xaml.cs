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
        AllocConsole();

        Services = ConfigureServices();

        var logger = Services.GetRequiredService<ILogger<App>>();
        logger.LogInformation("ScriptKiddie - WinUI 已启用调试控制台");

        this.InitializeComponent();
    }

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool AllocConsole();

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
        services.AddTransient<LoginPageModel>();
        services.AddSingleton<AccountManagePageModel>();
        services.AddTransient<SettingPageModel>();
        services.AddTransient<SelectSchedulePageModel>();
    }

    private static void AddServices(ServiceCollection services)
    {
        services.AddSingleton<IAppSettingsService, WindowsAppSettingsService>();
        services.AddSingleton<SelectScheduleProvider>();

        bool addMockServices = true;

        if (addMockServices)
        {
            services.AddSingleton<ILoginService, MockLoginService>();
            services.AddSingleton<IHttpClientProvider, MockHttpClientProvider>();
        }
        else
        {
            services.AddSingleton<ILoginService, UIALoginService>();
            services.AddSingleton<IHttpClientProvider, HttpClientProvider>();
        }

        services.AddSingleton<ICourseSelectService, CourseSelectService>();
        services.AddSingleton<AccountManageService>();

        services.AddSingleton<NavigationService>();
    }

    private static void AddViews(ServiceCollection services)
    {
        services.AddSingleton<MainWindow>();
    }

    private static void AddLogger(ServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.AddSimpleConsole(options =>
            {
                options.TimestampFormat = "F";
                options.IncludeScopes = false;
                options.ColorBehavior = LoggerColorBehavior.Enabled;
            });
            builder.SetMinimumLevel(LogLevel.Debug);
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
