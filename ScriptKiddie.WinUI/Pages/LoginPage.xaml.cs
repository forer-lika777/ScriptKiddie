using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using ScriptKiddie.WinUI.ViewModels;

namespace ScriptKiddie.WinUI.Pages;

public sealed partial class LoginPage : Page
{
    public LoginPageModel ViewModel { get; }

    public LoginPage()
    {
        InitializeComponent();
        ViewModel = App.Current.Services.GetRequiredService<LoginPageModel>();
    }
}
