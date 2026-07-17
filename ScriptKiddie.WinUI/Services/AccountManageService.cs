using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using ScriptKiddie.WinUI.Models;

namespace ScriptKiddie.WinUI.Services;

public class AccountManageService(ILoginService loginService, ICourseSelectService courseSelectService)
{
    private readonly ILoginService loginService = loginService;
    private readonly ICourseSelectService courseSelectService = courseSelectService;

    private bool hasLogin = false;

    public async Task<LoginResult> LoginAsync(LoginOption loginOption)
    {
        var result = await loginService.LoginAsync(loginOption);

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

    public void AddCourseSelectPlan(CourseItem course, DateTime openTime, CancellationToken cancellationToken, int interval = 100)
    {
        _ = courseSelectService.AddCourseSelectPlan(course, openTime, cancellationToken);
    }
}
