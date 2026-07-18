using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ScriptKiddie.WinUI.Models;
using ScriptKiddie.WinUI.Services;
using System;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.ViewModels;

public partial class LoginPageModel : ObservableObject
{
    private readonly AccountManageService accountManageService;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    public partial string UserName { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    public partial string Password { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsBusy { get; set; } = false;

    private bool CanLogin()
    {
        return !string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrWhiteSpace(Password) && !IsBusy;
    }

    public event EventHandler? LoginSucceeded;

    public LoginPageModel(AccountManageService accountManageService)
    {
        this.accountManageService = accountManageService;
    }

    [RelayCommand(CanExecute = nameof(CanLogin))]

    private async Task LoginAsync()
    {
        IsBusy = true;

        var loginOption = new LoginOption
        {
            UserName = this.UserName,
            Password = this.Password,
            LoadCookie = false
        };

        var result = await accountManageService.LoginAsync(loginOption);

        IsBusy = false;

        if (result.Success)
        {
            WeakReferenceMessenger.Default.Send(new LoginSuccessMessage());
        }
    }
}
