using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ScriptKiddie.WinUI.Models;
using ScriptKiddie.WinUI.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.ViewModels;

public partial class LoginPageModel : ObservableValidator
{
    private readonly AccountManageService accountManageService;

    [ObservableProperty]
    [Required(ErrorMessage = "用户名不能为空")]
    [NotifyPropertyChangedFor(nameof(UserNameError))]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    public partial string UserName { get; set; } = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "密码不能为空")]
    [NotifyPropertyChangedFor(nameof(PasswordError))]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    public partial string Password { get; set; } = string.Empty;

    [ObservableProperty]
    [CaptchaRequired(nameof(NeedCaptcha), ErrorMessage = "验证码不能为空")]
    [NotifyPropertyChangedFor(nameof(CaptchaError))]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    public partial string Captcha { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    public partial bool NeedCaptcha { get; set; } = false;

    private bool userNameBeenValid = false;
    
    private bool passwordBeenValid = false;

    private bool captchaBeenValid = false;

    private ValidationErrorStatus GetErrorStatus(string propertyName, ref bool beenValidFlag)
    {
        var propertyValue = GetType().GetProperty(propertyName)?.GetValue(this);
        ValidateProperty(propertyValue, propertyName);

        var errors = GetErrors(propertyName).ToList();
        bool currentIsValid = errors.Count == 0;

        // 如果当前校验通过，把传入的 bool 标志位置为 true
        if (currentIsValid)
        {
            beenValidFlag = true;
            return new ValidationErrorStatus();
        }

        // 如果当前有错，且曾经合法过，才暴露错误信息
        if (beenValidFlag)
        {
            return new ValidationErrorStatus(errors.FirstOrDefault()?.ErrorMessage ?? string.Empty);
        }

        return new ValidationErrorStatus();
    }

    public ValidationErrorStatus UserNameError => GetErrorStatus(nameof(UserName), ref userNameBeenValid);

    public ValidationErrorStatus PasswordError => GetErrorStatus(nameof(Password), ref passwordBeenValid);

    public ValidationErrorStatus CaptchaError => GetErrorStatus(nameof(Captcha), ref captchaBeenValid);

    [ObservableProperty]
    public partial Uri? CaptchaImage { get; set; } = null;

    [ObservableProperty]
    public partial string Message { get; set; } = string.Empty;

    private bool CanLogin()
    {
        ValidateAllProperties();
        return !HasErrors;
    }

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

    public partial class ValidationErrorStatus : ObservableObject
    {
        public ValidationErrorStatus() { }

        public ValidationErrorStatus(string message)
        {
            Message = message;
            Success = !string.IsNullOrWhiteSpace(message);
        }

        [ObservableProperty]
        public partial string Message { get; set; } = string.Empty;

        [ObservableProperty]
        public partial bool Success { get; private set; }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class CaptchaRequiredAttribute : ValidationAttribute
    {
        public string NeedCaptchaPropertyName;

        public CaptchaRequiredAttribute(string needCaptchaPropertyName)
        {
            NeedCaptchaPropertyName = needCaptchaPropertyName;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var property = validationContext.ObjectType.GetProperty(NeedCaptchaPropertyName);
            if (property == null)
            {
                return new ValidationResult($"未找到属性 {NeedCaptchaPropertyName}");
            }

            bool? needCaptchaValue = (bool?)property.GetValue(validationContext.ObjectInstance, null);

            if (needCaptchaValue == true)
            {
                var captchaStr = value as string;
                if (string.IsNullOrWhiteSpace(captchaStr))
                {
                    return new ValidationResult(ErrorMessage ?? "请输入验证码");
                }
            }

            return ValidationResult.Success;
        }
    }
}
