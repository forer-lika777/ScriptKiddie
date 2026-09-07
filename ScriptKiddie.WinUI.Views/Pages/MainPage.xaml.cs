using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ScriptKiddie.Core.Models;
using ScriptKiddie.Core.ViewModels;
using System.Linq;

namespace ScriptKiddie.WinUI.Views.Pages;

public sealed partial class MainPage : Page, IRecipient<UpdateLoginStatusMessage>
{
    private readonly ILogger<MainPage> logger;
    private string? currentPageTag = "home";

    public MainPageModel ViewModel { get; set; }

    public MainPage()
    {
        InitializeComponent();

        logger = AppServices.GetRequiredService<ILogger<MainPage>>();
        ViewModel = AppServices.GetRequiredService<MainPageModel>();
        AppServices.GetRequiredService<NavigationService>().Initialize(MainFrame);

        NavigationView.MenuItems.Add(new NavigationViewItem { Content = "开始", Icon = new SymbolIcon(Symbol.Home), Tag = "home" });
        NavigationView.MenuItems.Add(new NavigationViewItem { Content = "账户管理", Icon = new SymbolIcon(Symbol.Contact), Tag = "accountmanage" });
        NavigationView.MenuItems.Add(new NavigationViewItem { Content = "选课任务", Icon = new SymbolIcon(Symbol.List), Tag = "courselist" });
        NavigationView.MenuItems.Add(new NavigationViewItem { Content = "关于 WinUI", Icon = new SymbolIcon(Symbol.Find), Tag = "showacase" });
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
