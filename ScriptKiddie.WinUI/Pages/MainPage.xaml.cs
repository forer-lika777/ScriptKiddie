using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using ScriptKiddie.WinUI.Services;
using WinRT;

namespace ScriptKiddie.WinUI.Pages;

public sealed partial class MainPage : Page
{
    private string currentPageTag = "home";

    public MainPage()
    {
        InitializeComponent();
        App.Current.Services.GetRequiredService<NavigationService>().Initialize(MainFrame);
        MainFrame.Navigate(typeof(HomePage));
        NavigationView.SelectedItem = NavigationView.MenuItems[0];
    }

    private void navigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.IsSettingsInvoked)
        {
            currentPageTag = "settings";
            MainFrame.Navigate(typeof(SettingPage));
        }
        else
        {
            string tag = args.InvokedItemContainer.Tag.As<string>();
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
            MainFrame.GoBack();
    }
}
