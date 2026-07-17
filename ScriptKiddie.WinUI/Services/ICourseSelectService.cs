using ScriptKiddie.WinUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.Services;

public interface ICourseSelectService
{
    public Task<CourseResponse?> GetSelectableCoursesAsync();
    public Task<List<CourseItem>?> GetSelectedCoursesAsync();
    public Task AddCourseSelectPlan(CourseItem course, DateTime openTime, CancellationToken cancellationToken, int interval = 2000);
}
