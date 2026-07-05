using Script_Kiddie.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Script_Kiddie.Services;

class AccountManageService
{
    private readonly HttpClient httpClient;
    private readonly HttpClientHandler httpClientHandler;

    private readonly UIALoginService uialoginService;
    private readonly CourseSelectService courseSelectService;

    private bool hasLogin = false;

    public AccountManageService(IServiceProvider provider)
    {
        httpClientHandler = new HttpClientHandler
        {
            AllowAutoRedirect = true,
            UseCookies = true,
            CookieContainer = new CookieContainer(),
        };

        httpClient = new HttpClient(httpClientHandler);
        httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

        courseSelectService = new CourseSelectService(httpClient, httpClientHandler, provider.GetRequiredService<ILogger<CourseSelectService>>());
        uialoginService = new UIALoginService(httpClient, httpClientHandler, provider.GetRequiredService<ILogger<UIALoginService>>());
    }

    public async Task<LoginResult> LoginAsync(LoginOption loginOption)
    {
        var result = await uialoginService.LoginAsync(loginOption);

        hasLogin = result.Success;

        return result;
    }

    public async Task<CourseResponse?> GetSelectableCoursesAsync()
    {
        return await courseSelectService.GetSelectableCoursesAsync();
    }

    public async Task<List<CourseItem>?> GetSelectedCoursesAsync()
    {
        return await courseSelectService.GetSelectedCoursesAsync();
    }

    public async Task<bool> AddCourseSelectPlan(CourseItem course, DateTime openTime, CancellationToken cancellationToken, int interval = 100)
    {
        return await courseSelectService.AddCourseSelectPlan(course, openTime, cancellationToken);
    }
}
