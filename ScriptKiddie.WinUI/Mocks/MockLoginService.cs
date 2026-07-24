using ScriptKiddie.WinUI.Models;
using ScriptKiddie.WinUI.Services;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.Mocks;

public class MockLoginService : ILoginService
{
    private string captchaImageContent = "0721";

    public async Task<LoginResult> LoginAsync(LoginOption loginOption)
    {
        return await CaptchaLogin(loginOption);
    }

    private async Task<LoginResult> CommonLogin(LoginOption loginOption)
    {
        await Task.Delay(500);

        if (loginOption.UserName == "20260721" && loginOption.Password == "0d000721")
        {
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

        return new LoginResult
        {
            Message = "警告：您当前正在测试环境进行登录。请使用预设的信息来进行登录，并在接入正式环境之前退出此账户。\n用户名：20260721；密码：0d000721",
        };
    }

    private async Task<LoginResult> CaptchaLogin(LoginOption loginOption)
    {
        await Task.Delay(500);

        if (string.IsNullOrWhiteSpace(loginOption.Captcha))
        {
            return new LoginResult
            {
                Success = false,
                NeedCaptcha = true,
                Message = "要求输入验证码"
            };
        }

        await Task.Delay(1000);

        if (loginOption.Captcha != captchaImageContent)
        {
            return new LoginResult
            {
                Success = false,
                NeedCaptcha = true,
                Message = "验证码错误",
            };
        }

        if (loginOption.UserName == "20260721" && loginOption.Password == "0d000721")
        {
            return new LoginResult
            {
                Success = true,
                Grade = "80808080",
                AccountName = "WinUI 的受害者之二。",
                StatusCode = HttpStatusCode.OK,
                Message = "你已经登录成功了你知道吗？",
                ResponseContent = ".",
            };
        }

        return new LoginResult
        {
            Message = "警告：您当前正在测试环境进行登录。请使用预设的信息来进行登录，并在接入正式环境之前退出此账户。\n用户名：20260721；密码：0d000721",
        };
    }

    public async Task<bool> LogoutAsync()
    {
        await Task.Delay(1000);
        return true;
    }

    public string GetCaptchaImage()
    {
        return Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Mocks", "Data", "captcha1.png");
    }

    public string GetRandomCaptchaImage()
    {
        if (captchaImageContent == "0721")
        {
            captchaImageContent = "0d000721";
            return Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Mocks", "Data", "captcha2.png");
        }
        if (captchaImageContent == "0d000721")
        {
            captchaImageContent = "0721";
            return Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Mocks", "Data", "captcha1.png");
        }

        return Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Mocks", "Data", "captcha1.png");
    }
}
