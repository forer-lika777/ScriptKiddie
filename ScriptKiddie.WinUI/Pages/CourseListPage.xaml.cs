using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ScriptKiddie.WinUI.Models;
using ScriptKiddie.WinUI.Pages.Controls;
using ScriptKiddie.WinUI.ViewModels;
using System;

namespace ScriptKiddie.WinUI.Pages;

public sealed partial class CourseListPage : Page
{
    public CourseListPageModel ViewModel { get; set; }
    public CourseListPage()
    {
        InitializeComponent();
        ViewModel = App.Current.Services.GetRequiredService<CourseListPageModel>();
    }

    private async void ViewMoreButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ContentDialog dialog = new ContentDialog();

        dialog.XamlRoot = this.XamlRoot;
        dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
        dialog.PrimaryButtonText = "关闭";
        dialog.DefaultButton = ContentDialogButton.Primary;
        dialog.Content = new CourseInfoPanel((CourseItem)((Button)sender).DataContext);

        await dialog.ShowAsync();
    }
}
