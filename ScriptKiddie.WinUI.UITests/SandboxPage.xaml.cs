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

namespace ScriptKiddie.WinUI.UITests;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class SandboxPage : Page
{
    public SandboxPage()
    {
        InitializeComponent();

        var services = new ServiceCollection();

        var navigationService = new NavigationService();
        navigationService.Initialize(ContentFrame);

        services.AddSingleton<CourseListPageModel>();
        services.AddSingleton<MainPageModel>();
        services.AddSingleton<MainWindowModel>();
        services.AddTransient<LoginPageModel>();
        services.AddSingleton<AccountManagePageModel>();
        services.AddTransient<SettingPageModel>();
        services.AddTransient<SelectSchedulePageModel>();

        var settingService = new Mock<IAppSettingsService>();
        settingService.Setup(a => a.AccountInfo.Value).Returns(() => new AccountInfo());
        settingService.Setup(a => a.SelectSchedules.Value).Returns(() => []);
        settingService.Setup(a => a.Cookies.Value).Returns(() => []);
        settingService.Setup(a => a.Password.Value).Returns(() => string.Empty);
        settingService.Setup(a => a.IsLoggedIn.Value).Returns(() => false);
        services.AddSingleton<IAppSettingsService>(settingService.Object);

        App.Current.ConfigureServices(services);
    }

    private void LoadWelcomePage_Click(object sender, RoutedEventArgs e)
    {
        ContentFrame.Navigate(typeof(WelcomePage), null, new SuppressNavigationTransitionInfo());
    }
}
