using ScriptKiddie.WinUI.Models;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.Services;

public interface IHttpClientProvider
{
    public HttpClient GetCurrentClient();

    public void SetCookies(CookieCollection cookies);

    public CookieCollection GetCookies();

    public Task<CourseResponse> FetchSelectableCoursesAsync(CancellationToken cancellationToken);

    public Task<List<CourseItem>> FetchSelectedCoursesAsync(CancellationToken cancellationToken);

    public Task<int> FetchCourseSelectLimitCountAsync(CancellationToken cancellationToken);

    public Task<HttpResponseMessage> SendAddCourseRequestAsync(CourseItem course, CancellationToken cancellationToken);

    public Task<HttpResponseMessage> SendWithdrawCourseRequestAsync(CourseItem course, CancellationToken cancellationToken);

}
