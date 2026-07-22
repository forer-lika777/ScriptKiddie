using ScriptKiddie.WinUI.Models;
using ScriptKiddie.WinUI.Services;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.Mocks;

public class MockLoginService : ILoginService
{
    public async Task<LoginResult> LoginAsync(LoginOption loginOption)
    {
        return await CaptchaLogin(loginOption);
    }

    private async Task<LoginResult> CommonLogin(LoginOption loginOption)
    {
        await Task.Delay(500);

        return new LoginResult
        {
            Success = true,
            Grade = "80808080",
            AccountName = "WinUI 的受害者之一。",
            StatusCode = HttpStatusCode.OK,
            Message = "。",
            ResponseContent = ".",
        };
    }

    private async Task<LoginResult> CaptchaLogin(LoginOption loginOption)
    {
        await Task.Delay(500);

        //var stream = File.OpenRead(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Mocks", "Data", "captcha.png"));
        if (string.IsNullOrWhiteSpace(loginOption.Captcha))
        {
            return new LoginResult
            {
                Success = false,
                NeedCaptcha = true,
                Message = "要求输入验证码"
            };
        }

        await Task.Delay(2000);

        if (loginOption.Captcha != "0721")
        {
            return new LoginResult
            {
                Success = false,
                NeedCaptcha = true,
                Message = "验证码错误",
            };
        }

        return new LoginResult
        {
            Success = true,
            Grade = "80808080",
            AccountName = "WinUI 的受害者之二。",
            StatusCode = HttpStatusCode.OK,
            Message = "。",
            ResponseContent = ".",
        };
    }

    public async Task<bool> LogoutAsync()
    {
        await Task.Delay(1000);
        return true;
    }

    public string GetCaptchaImage()
    {
        return Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Mocks", "Data", "captcha.png");
    }

    public string GetRandomCaptchaImage()
    {
        return Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Mocks", "Data", "captcha.png");
    }
}
