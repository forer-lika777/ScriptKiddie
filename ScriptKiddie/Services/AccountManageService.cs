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

    private readonly UIALoginService uiaLoginService;
    private readonly CourseSelectService courseSelectService;

    private bool hasLogin = false;

    public AccountManageService(UIALoginService uiaLoginService, CourseSelectService courseSelectService)
    {
        this.courseSelectService = courseSelectService;
        this.uiaLoginService = uiaLoginService;
    }

    public async Task<LoginResult> LoginAsync(LoginOption loginOption)
    {
        var result = await uiaLoginService.LoginAsync(loginOption);

        hasLogin = result.Success;

        if (result.Success)
        {
            _ = KeepNotice();
        }

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

    private async Task KeepNotice()
    {

    }
}
