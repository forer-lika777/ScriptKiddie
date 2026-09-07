using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Moq;
using ScriptKiddie.WinUI.Mocks;
using ScriptKiddie.Core.Models;
using ScriptKiddie.WinUI.Pages;
using ScriptKiddie.WinUI.Services;
using ScriptKiddie.Core.Services;
using ScriptKiddie.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using ScriptKiddie.WinUI.Views;
using ScriptKiddie.WinUI.Views.Pages;

namespace ScriptKiddie.WinUI.UITests;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class SandboxPage : Page
{
    private bool isLoggedIn = false;

    public SandboxPage()
    {
        InitializeComponent();

        var services = new ServiceCollection();

        services.AddSingleton<CourseListPageModel>();
        services.AddSingleton<MainPageModel>();
        services.AddSingleton<MainWindowModel>();
        services.AddTransient<LoginPageModel>();
        services.AddSingleton<AccountManagePageModel>();
        services.AddTransient<SettingPageModel>();
        services.AddTransient<SelectSchedulePageModel>();

        services.AddSingleton<NavigationService>();
        services.AddSingleton<IAccountManageService, AccountManageService>();
        services.AddSingleton<ILoginService, MockLoginService>();

        services.AddSingleton<ICourseSelectService, CourseSelectService>();
        services.AddSingleton<ISelectScheduleProvider, SelectScheduleProvider>();
        services.AddSingleton<IHttpClientProvider, MockHttpClientProvider>();

        var settingService = new Mock<IAppSettingsService>();
        settingService.Setup(a => a.AccountInfo.Value).Returns(() => new AccountInfo());
        settingService.Setup(a => a.SelectSchedules.Value).Returns(() => []);
        settingService.Setup(a => a.Cookies.Value).Returns(() => []);
        settingService.Setup(a => a.Password.Value).Returns(() => string.Empty);
        settingService.Setup(a => a.IsLoggedIn.Value).Returns(() => isLoggedIn);
        settingService.SetupSet(a => a.IsLoggedIn.Value = true).Callback(() => isLoggedIn = true);
        settingService.SetupSet(a => a.IsLoggedIn.Value = false).Callback(() => isLoggedIn = false);
        services.AddSingleton<IAppSettingsService>(settingService.Object);

        App.Current.ConfigureServices(services);

        var navigationService = AppServices.GetRequiredService<NavigationService>();
        navigationService.Initialize(ContentFrame);
    }

    private void LoadWelcomePage_Click(object sender, RoutedEventArgs e)
    {
        ContentFrame.Navigate(typeof(WelcomePage), null, new SuppressNavigationTransitionInfo());
    }

    private async void LoadCourseListPage_Click(object sender, RoutedEventArgs e)
    {
        await AppServices.GetRequiredService<IAccountManageService>().LoginAsync(new LoginOption
        {
            UserName = "20260721",
            Password = "0d000721"
        });

        ContentFrame.Navigate(typeof(CourseListPage), null, new SuppressNavigationTransitionInfo());
    }

    private async void LoadMainPage_Click(object sender, RoutedEventArgs e)
    {
        await AppServices.GetRequiredService<IAccountManageService>().LoginAsync(new LoginOption
        {
            UserName = "20260721",
            Password = "0d000721"
        });
        AppServices.GetRequiredService<IAppSettingsService>().IsLoggedIn.Value = true;
        ContentFrame.Navigate(typeof(MainPage), null, new SuppressNavigationTransitionInfo());
    }

    private async void LoadLoginPage_Click(object sender, RoutedEventArgs e)
    {
        ContentFrame.Navigate(typeof(LoginPage), null, new SuppressNavigationTransitionInfo());
    }

    private void NavigationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        if (ContentFrame.CanGoBack)
            ContentFrame.GoBack();
    }
}
