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
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    public partial string Captcha { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool NeedCaptcha { get; set; } = false;

    [ObservableProperty]
    public partial Uri? CaptchaImage { get; set; } = null;

    [ObservableProperty]
    public partial string Message { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsBusy { get; set; } = false;

    private bool CanLogin()
    {
        if (NeedCaptcha)
        {
            return !string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrWhiteSpace(Password) && !string.IsNullOrWhiteSpace(Captcha) && !IsBusy;
        }
        return !string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrWhiteSpace(Password) && !IsBusy;
    }

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
            Captcha = this.Captcha,
            LoadCookie = false
        };

        var result = await accountManageService.LoginAsync(loginOption);

        IsBusy = false;
        Message = result.Message;

        if (result.Success)
        {
            WeakReferenceMessenger.Default.Send(new LoginSuccessMessage());
            return;
        }

        if (result.NeedCaptcha)
        {
            NeedCaptcha = true;
            CaptchaImage = new Uri(accountManageService.GetCaptchaImage());
        }
    }

    [RelayCommand]
    private void RefreshCaptcha()
    {
        CaptchaImage = new Uri(accountManageService.GetRandomCaptchaImage());
    }
}
