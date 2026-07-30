using CommunityToolkit.Mvvm.Messaging;
using ScriptKiddie.WinUI.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net;

namespace ScriptKiddie.WinUI.Services;

public class AccountManageService
{
    private readonly ILoginService loginService;
    private readonly ICourseSelectService courseSelectService;
    private readonly IAppSettingsService appSettingsService;
    private readonly HttpClientProvider httpClientProvider;

    private CancellationTokenSource? logoutCancellationTokenSource;
    private CancellationToken logoutCancellationToken;

    private AccountInfo? accountInfo;

    private bool hasLogin = false;

    public AccountManageService(ILoginService loginService, ICourseSelectService courseSelectService, IAppSettingsService appSettingsService, HttpClientProvider httpClientProvider)
    {
        this.loginService = loginService;
        this.courseSelectService = courseSelectService;
        this.appSettingsService = appSettingsService;
        this.httpClientProvider = httpClientProvider;
        Init();
    }

    private void Init()
    {
        if (appSettingsService.IsLoggedIn.Value)
        {
            logoutCancellationTokenSource = new CancellationTokenSource();
            logoutCancellationToken = logoutCancellationTokenSource.Token;
            var cookies = appSettingsService.Cookies.Value;
            httpClientProvider.SetCookies(cookies.ToCookieCollection());
            accountInfo = appSettingsService.AccountInfo.Value;
        }
    }

    public async Task<LoginResult> LoginAsync(LoginOption loginOption)
    {
        logoutCancellationTokenSource = new CancellationTokenSource();
        logoutCancellationToken = logoutCancellationTokenSource.Token;
        var result = await loginService.LoginAsync(loginOption);

        if (hasLogin = result.Success)
        {
            appSettingsService.IsLoggedIn.Value = true;
            appSettingsService.Cookies.Value = result.CookieContent.ToCookieItemList();

            accountInfo = new AccountInfo
            {
                AccountId = loginOption.UserName,
                AccountName = result.AccountName,
                Grade = result.Grade,
            };

            appSettingsService.AccountInfo.Value = accountInfo;

            WeakReferenceMessenger.Default.Send<AccountInfoChangedMessage>(new AccountInfoChangedMessage(accountInfo));
        }

        return result;
    }

    public async Task<bool> LogoutAsync()
    {
        logoutCancellationTokenSource?.Cancel();
        return await loginService.LogoutAsync();
    }

    public AccountInfo? GetAccountInfo()
    {
        return accountInfo;
    }

    public async Task<CourseResponse?> GetSelectableCoursesAsync()
    {
        return await courseSelectService.GetSelectableCoursesAsync(logoutCancellationToken);
    }

    public async Task<List<CourseItem>?> GetSelectedCoursesAsync()
    {
        return await courseSelectService.GetSelectedCoursesAsync(logoutCancellationToken);
    }

    public void AddCourseSelectPlan(CourseItem course, DateTime openTime, CancellationToken cancellationToken, int interval = 100)
    {
        _ = courseSelectService.AddCourseSelectPlan(course, openTime, cancellationToken);
    }

    public string GetCaptchaImage()
    {
        return loginService.GetCaptchaImage();
    }

    public string GetRandomCaptchaImage()
    {
        return loginService.GetRandomCaptchaImage();
    }
}
