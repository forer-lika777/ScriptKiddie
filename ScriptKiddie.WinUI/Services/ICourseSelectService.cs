using ScriptKiddie.WinUI.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.Services;

public interface ICourseSelectService
{
    public Task<CourseResponse?> GetSelectableCoursesAsync(CancellationToken cancellationToken);
    public Task<List<CourseItem>?> GetSelectedCoursesAsync(CancellationToken cancellationToken);
    public Task<int?> GetSelectLimitCountAsync(CancellationToken cancellationToken);
    public Task AddCourseSelectPlan(CourseItem course, DateTime openTime, CancellationToken cancellationToken, int interval = 2000);
}
