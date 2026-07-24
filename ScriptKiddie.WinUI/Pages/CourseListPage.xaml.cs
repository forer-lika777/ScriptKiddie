using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using ScriptKiddie.WinUI.ViewModels;

namespace ScriptKiddie.WinUI.Pages;

public sealed partial class CourseListPage : Page
{
    public CourseListPageModel ViewModel { get; set; }
    public CourseListPage()
    {
        InitializeComponent();
        ViewModel = App.Current.Services.GetRequiredService<CourseListPageModel>();
    }
}
