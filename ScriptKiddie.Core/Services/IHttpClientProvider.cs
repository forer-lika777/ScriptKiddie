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
    /// <summary>
    /// 设置可选课程列表。此方法仅在测试环境可用。
    /// </summary>
    /// <param name="courses"></param>
    public void SetSelectableCourses(ObservableCollection<CourseItem> courses);
    /// <summary>
    /// 设置已选课程列表。此方法仅在测试环境可用。
    /// </summary>
    /// <param name="courses"></param>
    public void SetSelectedCourses(ObservableCollection<CourseItem> courses);
    public Task<int> FetchCourseSelectLimitCountAsync(CancellationToken cancellationToken);
    public Task<HttpResponseMessage> SendAddCourseRequestAsync(CourseItem course, CancellationToken cancellationToken);
    public Task<HttpResponseMessage> SendWithdrawCourseRequestAsync(CourseItem course, CancellationToken cancellationToken);

}
