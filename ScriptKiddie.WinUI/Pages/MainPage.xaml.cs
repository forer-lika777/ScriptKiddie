using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using ScriptKiddie.WinUI.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ApplicationSettings;
using WinRT;

namespace ScriptKiddie.WinUI.Pages
{
    public sealed partial class MainPage : Page
    {
        private string currentPageTag = "home";

        public MainPage()
        {
            InitializeComponent();
            OpenHomePage();
            navigationView.SelectedItem = navigationView.MenuItems[0];
        }

        private void navigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                currentPageTag = "settings";
                OpenSettingsPage();
            }
            else
            {
                string tag = args.InvokedItemContainer.Tag.As<string>();
                if (currentPageTag == tag) return;
                currentPageTag = tag;
                if (tag == "home")
                {
                    OpenHomePage();
                }
                else if (tag == "accountmanage")
                {
                    OpenAccountManagePage();
                }
                else if (tag == "courselist")
                {
                    OpenCourseListPage();
                }
                else if (tag == "showacase")
                {
                    OpenShowacasePage();
                }
            }
        }

        private void OpenHomePage()
        {
            mainFrame.Navigate(typeof(HomePage));
        }

        private void OpenAccountManagePage()
        {
            mainFrame.Navigate(typeof(AccountManagePage));
        }

        private void OpenCourseListPage()
        {
            mainFrame.Navigate(typeof(CourseListPage));
        }

        private void OpenSettingsPage()
        {
            mainFrame.Navigate(typeof(SettingPage));
        }

        private void OpenShowacasePage()
        {
            mainFrame.Navigate(typeof(ShowacasePage));
        }
    }
}
