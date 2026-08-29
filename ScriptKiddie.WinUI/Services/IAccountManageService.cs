using ScriptKiddie.WinUI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.Services;

public interface IAccountManageService
{
    public Task<LoginResult> LoginAsync(LoginOption loginOption);
    public Task<bool> LogoutAsync(CancellationToken cts = default);
    public AccountInfo? GetAccountInfo();
    public Task<CourseResponse?> GetSelectableCoursesAsync();
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
