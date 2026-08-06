using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ScriptKiddie.WinUI.Models;
using ScriptKiddie.WinUI.Pages.Controls;
using ScriptKiddie.WinUI.Services;
using ScriptKiddie.WinUI.ViewModels;
using System;

namespace ScriptKiddie.WinUI.Pages;

public sealed partial class CourseListPage : Page
{
    private readonly NavigationService navigationService;

    public CourseListPageModel ViewModel { get; set; }

    // 用于在数据模板中的元素使用，因为他们只能通过静态资源访问 ViewModel 中的可观测属性。
    public static CourseListPageModel ViewModelCache { get; set; } = null!;
    public CourseListPage()
    {
        InitializeComponent();
        ViewModelCache = ViewModel = App.Current.Services.GetRequiredService<CourseListPageModel>();
        navigationService = App.Current.Services.GetRequiredService<NavigationService>();
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        // 离开时将存储的静态值设置为 null，以匹配页面的生命周期
        ViewModelCache = null!;
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

    private void ConfigureSelectScheduleButton_Click(object sender, RoutedEventArgs e)
    {
        navigationService.NavigateTo<SelectSchedulePage>();
    }
}