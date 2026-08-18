using ScriptKiddie.WinUI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.Services;

public interface ICourseSelectService
{
    public ObservableCollection<CourseSelectTask> GetSelectTasks();
    public Task<CourseResponse?> GetSelectableCoursesAsync(CancellationToken cancellationToken);
    public Task<List<CourseItem>?> GetSelectedCoursesAsync(CancellationToken cancellationToken);
    public Task<int?> GetSelectLimitCountAsync(CancellationToken cancellationToken);
    public bool AddCourse(CourseItem course, SelectSchedule selectSchedule, OperationType operationType);
    public bool AddCourse(CourseItem course, CourseItem courseToWithdraw, SelectSchedule selectSchedule);
    public bool RemoveCourse(CourseItem course);
    public Task BeginSyncCourses(CancellationToken cancellationToken);
    public Task StopSyncCourses();
}
