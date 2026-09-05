using ScriptKiddie.Core.Models;
using System.Collections.ObjectModel;

namespace ScriptKiddie.Core.Services;

public interface ICourseSelectService
{
    public ObservableCollection<CourseSelectTask> GetSelectTasks();
    public Task<CourseResponse?> GetSelectableCoursesAsync(CancellationToken cancellationToken);
    public Task<ObservableCollection<CourseItem>?> GetSelectedCoursesAsync(CancellationToken cancellationToken);
    public Task<int?> GetSelectLimitCountAsync(CancellationToken cancellationToken);
    public Task<bool> AddCourseAsync(CourseItem course, SelectSchedule selectSchedule, OperationType operationType);
    public Task<bool> AddCourseAsync(CourseItem course, CourseItem courseToWithdraw, SelectSchedule selectSchedule);
    public bool RemoveCourse(CourseItem course);
    public Task BeginSyncCourses(CancellationToken cancellationToken);
    public Task StopSyncCourses();
}
