using CommunityToolkit.Mvvm.Messaging;
using ScriptKiddie.WinUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.Services;

public class AccountManageService
{
    private readonly ILoginService loginService;
    private readonly ICourseSelectService courseSelectService;
    private readonly AppSettingsService appSettingsService;

    private AccountInfo? accountInfo;

    private bool hasLogin = false;

    public AccountManageService(ILoginService loginService, ICourseSelectService courseSelectService, AppSettingsService appSettingsService)
    {
        this.loginService = loginService;
        this.courseSelectService = courseSelectService;
        this.appSettingsService = appSettingsService;
    }

    public async Task<LoginResult> LoginAsync(LoginOption loginOption)
    {
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
        return await loginService.LogoutAsync();
    }

    public AccountInfo? GetAccountInfo()
    {
        return accountInfo;
    }

    public async Task<CourseResponse?> GetSelectableCoursesAsync()
    {
        return await courseSelectService.GetSelectableCoursesAsync();
    }

    public async Task<List<CourseItem>?> GetSelectedCoursesAsync()
    {
        return await courseSelectService.GetSelectedCoursesAsync();
    }

    public void AddCourseSelectPlan(CourseItem course, DateTime openTime, CancellationToken cancellationToken, int interval = 100)
    {
        _ = courseSelectService.AddCourseSelectPlan(course, openTime, cancellationToken);
    }
}
