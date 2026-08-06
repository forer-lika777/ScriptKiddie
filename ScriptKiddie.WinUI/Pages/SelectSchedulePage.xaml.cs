using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ScriptKiddie.WinUI.Models;
using ScriptKiddie.WinUI.Pages.Controls;
using ScriptKiddie.WinUI.Services;
using ScriptKiddie.WinUI.ViewModels;
using System;
using System.Collections.Generic;

namespace ScriptKiddie.WinUI.Pages;

public sealed partial class SelectSchedulePage : Page
{
    private readonly SelectScheduleProvider selectScheduleProvider;

    public SelectSchedulePageModel ViewModel { get; set; }

    public SelectSchedulePage()
    {
        InitializeComponent();
        selectScheduleProvider = App.Current.Services.GetRequiredService<SelectScheduleProvider>();
        ViewModel = App.Current.Services.GetRequiredService<SelectSchedulePageModel>();
        RefreshCommandButtonStatus();
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
        ContentDialog dialog = new ContentDialog();

        dialog.XamlRoot = this.XamlRoot;
        dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
        dialog.PrimaryButtonText = "确定";
        dialog.SecondaryButtonText = "取消";
        dialog.DefaultButton = ContentDialogButton.Primary;
        dialog.Title = "确定要删除此项吗？";
        dialog.Content = "此操作不可逆";

        dialog.PrimaryButtonClick += Dialog_PrimaryButtonClick;

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
        selectScheduleProvider.RemoveRange(schedules);

    }

    public static string SelectTypeToString(SelectType selectType)
    {
        if (selectType == SelectType.SelectAndWithdraw)
        {
            return "选课与退选";
        }
        else if (selectType == SelectType.SelectOnly)
        {
            return "仅选课";
        }
        if (selectType == SelectType.WithdrawOnly)
        {
            return "仅退选";
        }
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
