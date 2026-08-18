using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ScriptKiddie.WinUI.Models;
using ScriptKiddie.WinUI.Pages.Controls;
using ScriptKiddie.WinUI.Services;
using ScriptKiddie.WinUI.ViewModels;
using System;
using System.Collections.Generic;

namespace ScriptKiddie.WinUI.Pages;

public sealed partial class SelectSchedulePage : Page, IRecipient<SelectScheduleRemoveConfirmMessage>
{
    private readonly SelectScheduleProvider selectScheduleProvider;
    private readonly ICourseSelectService courseSelectService;

    public SelectSchedulePageModel ViewModel { get; set; }

    public SelectSchedulePage()
    {
        InitializeComponent();
        courseSelectService = App.Current.Services.GetRequiredService<ICourseSelectService>();
        selectScheduleProvider = App.Current.Services.GetRequiredService<SelectScheduleProvider>();
        ViewModel = App.Current.Services.GetRequiredService<SelectSchedulePageModel>();
        WeakReferenceMessenger.Default.Register<SelectScheduleRemoveConfirmMessage>(this);
        RefreshCommandButtonStatus();
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        // 订阅多次会重复发消息，要取消订阅
        WeakReferenceMessenger.Default.Unregister<SelectScheduleRemoveConfirmMessage>(this);
    }

    private void AddButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var panel = new SelectScheduleEditPanel(RootGrid);
        panel.Show();
    }

    private void EditButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var schedule = ScheduleItemList.SelectedItem as SelectSchedule;

        if (schedule is not null)
        {
            SelectScheduleEditPanelModel viewModel = new SelectScheduleEditPanelModel(schedule);

            var panel = new SelectScheduleEditPanel(RootGrid, viewModel);

            panel.Show();
        }
    }

    private async void DeleteButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        List<SelectSchedule> schedules = [];
        foreach (var item in ScheduleItemList.SelectedItems)
        {
            if (item is SelectSchedule selectSchedule)
            {
                schedules.Add(selectSchedule);
            }
        }

        await selectScheduleProvider.RemoveRange(schedules);
    }

    public async void Receive(SelectScheduleRemoveConfirmMessage message)
    {
        var panel = new SelectScheduleDeleteConfirmPanel(message.ChangedCourseSelectTasks);

        var dialog = new ContentDialog
        {
            XamlRoot = this.XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            PrimaryButtonText = "确认",
            SecondaryButtonText = "取消",
            Title = "确定要删除此时间表吗？",
            DefaultButton = ContentDialogButton.Primary,
            Content = panel,
        };

        dialog.PrimaryButtonClick += (sender, e) => message.TaskCompletionSource.SetResult();
        dialog.SecondaryButtonClick += (sender, e) => message.TaskCompletionSource.SetCanceled();

        await dialog.ShowAsync();
    }

    private void Dialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        List<SelectSchedule> schedules = [];
        foreach (var item in ScheduleItemList.SelectedItems)
        {
            if (item is SelectSchedule selectSchedule)
            {
                schedules.Add(selectSchedule);
            }
        }

        _ = selectScheduleProvider.RemoveRange(schedules);
    }

    public static string SelectTypeToString(SelectType selectType)
    {
        if (selectType == SelectType.SelectAndWithdraw)
            return "选课与退选";

        if (selectType == SelectType.SelectOnly)
            return "仅选课";

        if (selectType == SelectType.WithdrawOnly)
            return "仅退选";

        return string.Empty;
    }

    private void ScheduleItemList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RefreshCommandButtonStatus();
    }

    private void RefreshCommandButtonStatus()
    {
        if (ScheduleItemList.SelectedItems.Count == 0)
        {
            EditButton.IsEnabled = false;
            DeleteButton.IsEnabled = false;
        }
        else if (ScheduleItemList.SelectedItems.Count == 1)
        {
            EditButton.IsEnabled = true;
            DeleteButton.IsEnabled = true;
        }
        else if (ScheduleItemList.SelectedItems.Count > 1)
        {
            EditButton.IsEnabled = false;
            DeleteButton.IsEnabled = true;
        }
    }
}
