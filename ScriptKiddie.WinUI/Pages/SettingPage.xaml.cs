using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using ScriptKiddie.WinUI.ViewModels;

namespace ScriptKiddie.WinUI.Pages;

public sealed partial class SettingPage : Page
{
    public SettingPageModel ViewModel { get; set; }

    public SettingPage()
    {
        InitializeComponent();
        ViewModel = App.Current.Services.GetRequiredService<SettingPageModel>();
    }
}
