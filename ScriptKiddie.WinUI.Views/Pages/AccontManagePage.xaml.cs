using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using ScriptKiddie.Core.ViewModels;

namespace ScriptKiddie.WinUI.Views.Pages;

public sealed partial class AccountManagePage : Page
{
    public AccountManagePageModel ViewModel { get; set; }

    public AccountManagePage()
    {
        InitializeComponent();
        ViewModel = AppServices.GetRequiredService<AccountManagePageModel>();
    }
}
