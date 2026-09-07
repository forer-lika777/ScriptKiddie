using ScriptKiddie.Core.Models;
using System.Collections.ObjectModel;
using System.Net;

namespace ScriptKiddie.Core.Services;

public interface IHttpClientProvider
{
    public HttpClient GetCurrentClient();

    public void SetCookies(CookieCollection cookies);

    public CookieCollection GetCookies();

    public Task<ObservableCollection<CourseItem>?> FetchSelectableCoursesAsync(CancellationToken cancellationToken);

    public Task<ObservableCollection<CourseItem>> FetchSelectedCoursesAsync(CancellationToken cancellationToken);

    public Task<int> FetchCourseSelectLimitCountAsync(CancellationToken cancellationToken);

    public Task<HttpResponseMessage> SendAddCourseRequestAsync(CourseItem course, CancellationToken cancellationToken);

    public Task<HttpResponseMessage> SendWithdrawCourseRequestAsync(CourseItem course, CancellationToken cancellationToken);

}
