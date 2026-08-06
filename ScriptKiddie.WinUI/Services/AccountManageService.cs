using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using ScriptKiddie.WinUI.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.Services;

public partial class AccountManageService
{
    private readonly ILoginService loginService;
    private readonly ICourseSelectService courseSelectService;
    private readonly IAppSettingsService appSettingsService;
    private readonly HttpClientProvider httpClientProvider;

    private readonly ILogger<AccountManageService> logger;

    private TaskCompletionSource<bool>? loginTcs;

    private CancellationTokenSource? loggedOutCts;

    private AccountInfo? accountInfo;

    private bool isLoggedIn = false;

    public AccountManageService(ILoginService loginService, ICourseSelectService courseSelectService, IAppSettingsService appSettingsService, HttpClientProvider httpClientProvider, ILogger<AccountManageService> logger)
    {
        this.loginService = loginService;
        this.courseSelectService = courseSelectService;
        this.appSettingsService = appSettingsService;
        this.httpClientProvider = httpClientProvider;
        this.logger = logger;
        Init();
    }

    private async void Init()
    {
        if (appSettingsService.IsLoggedIn.Value)
        {
            ResetCts(ref loggedOutCts);

            loginTcs = new TaskCompletionSource<bool>();

            var cookies = appSettingsService.Cookies.Value.ToCookieCollection();
            var accountInfo = this.accountInfo = appSettingsService.AccountInfo.Value;
            string password = appSettingsService.Password.Value;

            var option = new LoginOption
            {
                UserName = accountInfo.AccountId,
                Password = password,
                CookieContent = cookies,
                LoadCookie = true,
                ExportCookie = true,
                RememberMe = true,
            };

            int retryCount = 0;

            while (retryCount < 20 && !loggedOutCts!.IsCancellationRequested)
            {
                var result = await loginService.LoginAsync(option, loggedOutCts!.Token);

                if (result.Success)
                {
                    isLoggedIn = true;

                    //WeakReferenceMessenger.Default.Send<AccountInfoChangedMessage>(new AccountInfoChangedMessage(accountInfo));

                    loginTcs?.TrySetResult(true);
                    loginTcs = null; // 清理

                    break;
                }

                retryCount++;
                logger.LogError("自动登录失败。无法使用保存的信息登录账户。当前重试次数为 {Count}", retryCount);

                if (result.NeedCaptcha)
                {
                    logger.LogError("请返回登录页面输入验证码，然后重新登录到账户。");
                    WeakReferenceMessenger.Default.Send<AutoLoginFailedNeedCaptchaMessage>(new AutoLoginFailedNeedCaptchaMessage());
                    break;
                }

                if (retryCount >= 20)
                {
                    logger.LogError("自动登录失败。已达到最大重试次数。");
                    loginTcs?.TrySetResult(false);
                    loginTcs = null;
                    break;
                }

                logger.LogInformation("等待10秒后重试。");
                await Task.Delay(10000);
            }
        }
    }

    public async Task<LoginResult> LoginAsync(LoginOption loginOption)
    {
        ResetCts(ref loggedOutCts);

        var result = await loginService.LoginAsync(loginOption, loggedOutCts!.Token);

        isLoggedIn = result.Success;

        if (isLoggedIn)
        {
            appSettingsService.IsLoggedIn.Value = true;

            var cookies = result.CookieContent.ToCookieItemList();
            if (cookies.Count > 0)
            {
                appSettingsService.Cookies.Value = cookies;
            }

            accountInfo = new AccountInfo
            {
                AccountId = loginOption.UserName,
                AccountName = result.AccountName,
                Grade = result.Grade,
            };

            appSettingsService.AccountInfo.Value = accountInfo;
            appSettingsService.Password.Value = loginOption.Password;

            loginTcs?.TrySetResult(true);
            loginTcs = null;

            ResetCts(ref loggedOutCts);

            WeakReferenceMessenger.Default.Send<AccountInfoChangedMessage>(new AccountInfoChangedMessage(accountInfo));
        }
        else
        {
            loginTcs?.TrySetResult(false);
            loginTcs = null;
        }

        return result;
    }

    public async Task<bool> LogoutAsync(CancellationToken cts = default)
    {
        if (!isLoggedIn)
        {
            logger.LogError("没有执行退出登录，因为当前已是登出状态。");
            loggedOutCts?.Cancel();
            return true;
        }

        bool success = await loginService.LogoutAsync(cts);

        if (success)
        {
            appSettingsService.IsLoggedIn.Value = false;
        }

        return success;
    }

    public AccountInfo? GetAccountInfo()
    {
        return accountInfo;
    }

    public async Task<CourseResponse?> GetSelectableCoursesAsync()
    {
        if (!await EnsureLoggedInAsync())
            return null;

        return await courseSelectService.GetSelectableCoursesAsync(loggedOutCts!.Token);
    }

    public async Task<List<CourseItem>?> GetSelectedCoursesAsync()
    {
        if (!await EnsureLoggedInAsync())
            return null;

        return await courseSelectService.GetSelectedCoursesAsync(loggedOutCts!.Token);
    }

    public async Task BeginSyncCourses()
    {
        _ = courseSelectService.BeginSyncCourses(loggedOutCts!.Token);
    }

    public async Task StopSyncCourses()
    {
        _ = courseSelectService.StopSyncCourses();
    }

    public async Task<int?> GetSelectLimitCountAsync()
    {
        if (!await EnsureLoggedInAsync())
            return null;

        return await courseSelectService.GetSelectLimitCountAsync(loggedOutCts!.Token);
    }

    /// <summary>
    /// 辅助方法：确保登录完成后再继续，如果未登录且正在重试登录，则异步等待
    /// </summary>
    private async Task<bool> EnsureLoggedInAsync(CancellationToken cancellationToken = default)
    {
        // 如果已经登录，直接放行
        if (isLoggedIn)
            return true;

        // 如果当前正在自动登录中（loginTcs 不为空），异步挂起等待它完成
        if (loginTcs is not null)
        {
            logger.LogInformation("当前正在登录中，请求已挂起等待...");

            try
            {
                // .NET 6+ 提供的 WaitAsync 可以完美只取消当前等待，不影响 TCS 本身
                return await loginTcs.Task.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // 仅当前请求被取消，不影响其他人
                return false;
            }
        }

        // 既没登录，也没有在自动登录，直接拦截
        logger.LogError("请求失败。当前实例未登录且未在登录状态中。");
        return false;
    }

    private static void ResetCts(ref CancellationTokenSource? cts, CancellationTokenSource? ctsToUse = null)
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = ctsToUse ?? new CancellationTokenSource();
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
