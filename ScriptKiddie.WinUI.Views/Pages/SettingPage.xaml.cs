using Microsoft.UI.Xaml.Controls;
using ScriptKiddie.Core.ViewModels;

namespace ScriptKiddie.WinUI.Views.Pages;

public sealed partial class SettingPage : Page
{
    public SettingPageModel ViewModel { get; set; }

    public SettingPage()
    {
        InitializeComponent();
        ViewModel = AppServices.GetRequiredService<SettingPageModel>();
    }
}
