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
    [NotifyPropertyChangedFor(nameof(UserNameError))]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    public partial string UserName { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PasswordError))]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    public partial string Password { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CaptchaError))]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    public partial string Captcha { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    public partial bool NeedCaptcha { get; set; } = false;

    private bool userNameBeenValid = false;
    private bool passwordBeenValid = false;
    private bool captchaBeenValid = false;

    /// <summary>校验用户名：不能为空。</summary>
    private static string? ValidateUserName(string? value) => string.IsNullOrWhiteSpace(value) ? "用户名不能为空" : null;

    /// <summary>校验密码：不能为空。</summary>
    private static string? ValidatePassword(string? value) => string.IsNullOrWhiteSpace(value) ? "密码不能为空" : null;

    /// <summary>
    /// 执行校验并返回状态。仅在曾经合法过且当前有错时才暴露错误信息。
    /// </summary>
    private static ValidationErrorStatus Validate(string? value, Func<string?, string?> validateFunc, ref bool beenValidFlag)
    {
        string? error = validateFunc(value);
        if (error == null)
        {
            beenValidFlag = true;
            return new ValidationErrorStatus();
        }
        return beenValidFlag ? new ValidationErrorStatus(error) : new ValidationErrorStatus();
    }

    public ValidationErrorStatus UserNameError => Validate(UserName, ValidateUserName, ref userNameBeenValid);

    public ValidationErrorStatus PasswordError => Validate(Password, ValidatePassword, ref passwordBeenValid);

    public ValidationErrorStatus CaptchaError
    {
        get
        {
            if (!NeedCaptcha)
            {
                captchaBeenValid = true;
                return new ValidationErrorStatus();
            }

            bool hasError = string.IsNullOrWhiteSpace(Captcha);
            if (!hasError)
            {
                captchaBeenValid = true;
                return new ValidationErrorStatus();
            }
            return captchaBeenValid ? new ValidationErrorStatus("验证码不能为空") : new ValidationErrorStatus();
        }
    }

    [ObservableProperty]
    public partial Uri? CaptchaImage { get; set; } = null;

    [ObservableProperty]
    public partial string Message { get; set; } = string.Empty;

    private bool CanLogin() => UserNameError.Success && PasswordError.Success && CaptchaError.Success && userNameBeenValid && passwordBeenValid && captchaBeenValid;

    public LoginPageModel(AccountManageService accountManageService)
    {
        this.accountManageService = accountManageService;
    }

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginAsync()
    {
        var loginOption = new LoginOption
        {
            UserName = this.UserName,
            Password = this.Password,
            Captcha = this.Captcha,
            ExportCookie = true,
            LoadCookie = false
        };

        var result = await accountManageService.LoginAsync(loginOption);
        Message = result.Message;

        if (result.Success)
        {
            WeakReferenceMessenger.Default.Send(new LoginSuccessMessage());
            return;
        }

        if (result.NeedCaptcha)
        {
            RefreshCaptcha();
            NeedCaptcha = true;
        }
    }

    [RelayCommand]
    private void RefreshCaptcha()
    {
        captchaBeenValid = false;
        Captcha = "";
        CaptchaImage = new Uri(accountManageService.GetRandomCaptchaImage());
    }

    /// <summary>表示单个属性的校验结果状态。</summary>
    public partial class ValidationErrorStatus : ObservableObject
    {
        public ValidationErrorStatus(string? message = null)
        {
            Message = message ?? string.Empty;
            Success = string.IsNullOrWhiteSpace(message);
        }

        [ObservableProperty]
        public partial string Message { get; set; } = string.Empty;

        [ObservableProperty]
        public partial bool Success { get; private set; }
    }
}
