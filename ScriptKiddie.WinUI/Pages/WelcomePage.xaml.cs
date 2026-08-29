using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ScriptKiddie.WinUI.Services;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;

namespace ScriptKiddie.WinUI.Pages;

public sealed partial class WelcomePage : Page, IRecipient<UpdateLoginStatusMessage>
{
    private readonly List<string> tabTags = ["tab1", "tab2", "tab3"];
    private readonly List<string> tabNames = ["欢迎", "使用说明", "账户登录"];
    private readonly NavigationService navigationService;

    private string currentTabTag;

    private bool isLoggedIn = false;

    private bool tabBarInitialized = false;

    public WelcomePage()
    {
        InitializeComponent();
        currentTabTag = tabTags[0];
        PreviousButton.Visibility = Visibility.Collapsed;
        navigationService = App.Current.Services.GetRequiredService<NavigationService>();
        TabSelectorBar.Items.Add(new SelectorBarItem
        {
            Name = tabTags[0],
            Text = tabNames[0],
            Tag = tabTags[0],
            IsSelected = true,
        });
        Tab2Content.Visibility = Visibility.Collapsed;
        Tab3Content.Visibility = Visibility.Collapsed;

        WeakReferenceMessenger.Default.Register<UpdateLoginStatusMessage>(this);
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        WeakReferenceMessenger.Default.Unregister<UpdateLoginStatusMessage>(this);
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        _ = AnimationService.AnimateMotionEnterAsync(Tab1Content, 40, 0);
        _ = AnimationService.AnimateOpacityEnterAsync(BackgroundImage);
    }

    private void PreviousButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        int next = tabTags.IndexOf(currentTabTag) - 1;

        TabSelectorBar.Items[next].IsSelected = true;
    }

    private void NextButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (tabTags.IndexOf(currentTabTag) == tabTags.Count - 1)
        {
            navigationService.NavigateTo<MainPage>();
            return;
        }

        int next = tabTags.IndexOf(currentTabTag) + 1;
        string tag = tabTags[next];

        if (next + 1 > TabSelectorBar.Items.Count)
        {
            var item = new SelectorBarItem
            {
                Name = tag,
                Text = tabNames[next],
                Tag = tag,
                IsSelected = true,
            };

            TabSelectorBar.Items.Add(item);
        }
        else
        {
            TabSelectorBar.Items[next].IsSelected = true;
        }
    }

    private void TabSelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        if (!tabBarInitialized)
        {
            tabBarInitialized = true;
            return;
        }

        var selectedItem = sender.SelectedItem as SelectorBarItem;
        if (selectedItem == null)
            return;

        if (selectedItem.Tag is not string tag)
            return;

        if (tag == currentTabTag)
            return;

        GotoTab(tag);

        currentTabTag = tag;
    }

    private void GotoTab(string tag)
    {
        int current = tabTags.IndexOf(currentTabTag);
        int next = tabTags.IndexOf(tag);

        if (next == 0)
        {
            PreviousButton.Visibility = Visibility.Collapsed;
        }
        else
        {
            PreviousButton.Visibility = Visibility.Visible;
        }

        if (next == tabTags.Count - 1 && !isLoggedIn)
        {
            NextButton.IsEnabled = false;
        }
        else
        {
            NextButton.IsEnabled = true;
        }

        if (current > next)
        {
            _ = AnimationService.AnimateMotionExitAsync(TabItem(current), 10, 0, 200);
            _ = AnimationService.AnimateMotionEnterAsync(TabItem(next), -50, 0, 700);
        }
        else
        {
            _ = AnimationService.AnimateMotionExitAsync(TabItem(current), -10, 0, 200);
            _ = AnimationService.AnimateMotionEnterAsync(TabItem(next), 50, 0, 700);
        }

        currentTabTag = tag;
    }

#pragma warning disable CA1859 // 尽可能使用具体类型以提高性能
    private UIElement TabItem(int index)
#pragma warning restore CA1859 // 尽可能使用具体类型以提高性能
    {
        if (index == 0)
            return Tab1Content;

        if (index == 1)
            return Tab2Content;

        if (index == 2)
            return Tab3Content;

        throw new ArgumentOutOfRangeException(nameof(index));
    }

    public void Receive(UpdateLoginStatusMessage message)
    {
        isLoggedIn = message.IsLoggedIn;

        if (tabTags.IndexOf(currentTabTag) == tabTags.Count - 1 && message.IsLoggedIn)
        {
            NextButton.IsEnabled = true;
        }
    }
}
