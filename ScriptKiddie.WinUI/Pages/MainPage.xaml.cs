using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ScriptKiddie.WinUI.Services;
using ScriptKiddie.WinUI.ViewModels;
using System;
using System.ComponentModel;
using System.Linq;

namespace ScriptKiddie.WinUI.Pages;

public sealed partial class MainPage : Page, IRecipient<UpdateLoginStatusMessage>
{
    private readonly ILogger<MainPage> logger;
    private string? currentPageTag = "home";

    public MainPageModel ViewModel { get; set; }

    public MainPage()
    {
        InitializeComponent();
        logger = App.Current.Services.GetRequiredService<ILogger<MainPage>>();
        ViewModel = App.Current.Services.GetRequiredService<MainPageModel>();
        App.Current.Services.GetRequiredService<NavigationService>().Initialize(MainFrame);
        NavigationView.SelectedItem = NavigationView.MenuItems[0];

        WeakReferenceMessenger.Default.Register<UpdateLoginStatusMessage>(this);

        UpdateStatus(ViewModel.IsLoggedIn);
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        WeakReferenceMessenger.Default.Unregister<UpdateLoginStatusMessage>(this);
    }

    public void Receive(UpdateLoginStatusMessage message)
    {
        UpdateStatus(message.IsLoggedIn);
    }

    private void UpdateStatus(bool isLoggedIn)
    {
        if (isLoggedIn)
        {
            MainFrame.Navigate(typeof(HomePage));
        }
        else
        {
            MainFrame.Navigate(typeof(WelcomePage));
        }
    }

    private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.IsSettingsInvoked)
        {
            currentPageTag = "settings";
            MainFrame.Navigate(typeof(SettingPage));
        }
        else
        {
            string? tag = args.InvokedItemContainer.Tag.ToString();

            if (currentPageTag == tag)
                return;

            currentPageTag = tag;

            if (tag == "home")
            {
                MainFrame.Navigate(typeof(HomePage));
            }
            else if (tag == "accountmanage")
            {
                MainFrame.Navigate(typeof(AccountManagePage));
            }
            else if (tag == "courselist")
            {
                MainFrame.Navigate(typeof(CourseListPage));
            }
            else if (tag == "showacase")
            {
                MainFrame.Navigate(typeof(ShowacasePage));
            }
        }
    }

    private void NavigationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        if (MainFrame.CanGoBack)
        {
            var page = MainFrame.BackStack.Last().SourcePageType;

            if (page.Equals(typeof(HomePage)))
            {
                currentPageTag = "home";
                NavigationView.SelectedItem = NavigationView.MenuItems[0];
            }
            else if (page.Equals(typeof(AccountManagePage)))
            {
                currentPageTag = "accountmanage";
                NavigationView.SelectedItem = NavigationView.MenuItems[1];
            }
            else if (page.Equals(typeof(CourseListPage)))
            {
                currentPageTag = "courselist";
                NavigationView.SelectedItem = NavigationView.MenuItems[2];
            }
            else if (page.Equals(typeof(ShowacasePage)))
            {
                currentPageTag = "showacase";
                NavigationView.SelectedItem = NavigationView.MenuItems[3];
            }

            MainFrame.GoBack();
        }
    }
}
