using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using ScriptKiddie.WinUI.Services;
using System.Linq;

namespace ScriptKiddie.WinUI.Pages;

public sealed partial class MainPage : Page
{
    private readonly ILogger<MainPage> logger;
    private string? currentPageTag = "home";

    public MainPage()
    {
        InitializeComponent();
        logger = App.Current.Services.GetRequiredService<ILogger<MainPage>>();
        App.Current.Services.GetRequiredService<NavigationService>().Initialize(MainFrame);
        MainFrame.Navigate(typeof(HomePage));
        NavigationView.SelectedItem = NavigationView.MenuItems[0];
    }

    private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        //logger.LogDebug("导航到页面：{args}", args.InvokedItemContainer.Tag.ToString());

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
            //logger.LogDebug("回退到页面：{page}", page);

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
