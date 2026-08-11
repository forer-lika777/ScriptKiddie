using Microsoft.Extensions.DependencyInjection;
using ScriptKiddie.WinUI.Models;
using ScriptKiddie.WinUI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.Mocks;

public class MockHttpClientProvider : IHttpClientProvider
{
    private CookieCollection cookies = [];
    private readonly SelectScheduleProvider selectScheduleProvider = App.Current.Services.GetRequiredService<SelectScheduleProvider>();

    public Task<int> FetchCourseSelectLimitCountAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<CourseResponse> FetchSelectableCoursesAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<List<CourseItem>> FetchSelectedCoursesAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public CookieCollection GetCookies()
    {
        return cookies;
    }

    public HttpClient GetCurrentClient()
    {
        throw new NotImplementedException();
    }

    public Task<HttpResponseMessage> SendAddCourseRequestAsync(CourseItem course, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<HttpResponseMessage> SendWithdrawCourseRequestAsync(CourseItem course, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void SetCookies(CookieCollection cookies)
    {
        this.cookies = cookies;
    }
}
