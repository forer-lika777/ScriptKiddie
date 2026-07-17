using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using ScriptKiddie.WinUI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.ViewModels;

public partial class AccountManagePageModel(AccountManageService accountManageService) : ObservableObject
{
    private readonly AccountManageService accountManageService = accountManageService;

    [ObservableProperty]
    public partial string AccountName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AccountId { get; set; } = string.Empty;

    [RelayCommand]
    private void ExitLogin()
    {
        var mainWindowModel = App.Current.Services.GetRequiredService<MainWindowModel>();
        mainWindowModel.IsLoggedIn = false;
    }
}
