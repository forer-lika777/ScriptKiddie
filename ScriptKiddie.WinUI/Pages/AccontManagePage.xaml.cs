using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using ScriptKiddie.WinUI.ViewModels;

namespace ScriptKiddie.WinUI.Pages;

public sealed partial class AccountManagePage : Page
{
    public AccountManagePageModel ViewModel { get; set; }

    public AccountManagePage()
    {
        InitializeComponent();
        ViewModel = App.Current.Services.GetRequiredService<AccountManagePageModel>();
    }
}
