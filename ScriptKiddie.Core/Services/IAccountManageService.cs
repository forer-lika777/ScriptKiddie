using ScriptKiddie.Core.Models;
using System.Collections.ObjectModel;

namespace ScriptKiddie.Core.Services;

public interface IAccountManageService
{
    public Task<LoginResult> LoginAsync(LoginOption loginOption);
    public Task<bool> LogoutAsync(CancellationToken cts = default);
    public AccountInfo? GetAccountInfo();
    public Task<ObservableCollection<CourseItem>?> GetSelectableCoursesAsync();
    public Task<ObservableCollection<CourseItem>?> GetSelectedCoursesAsync();
    public Task BeginSyncCourses();
    public Task StopSyncCourses();
    public Task<bool> AddCourseAsync(CourseItem course, SelectSchedule schedule, OperationType operationType);
    public Task<bool> AddCourseAsync(CourseItem course, CourseItem courseToWithdraw, SelectSchedule selectSchedule);
    public bool RemoveCourse(CourseItem course);
    public Task<int?> GetSelectLimitCountAsync();
    public string GetCaptchaImage();
    public string GetRandomCaptchaImage();
}
