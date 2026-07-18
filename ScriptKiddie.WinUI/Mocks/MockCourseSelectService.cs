using ScriptKiddie.WinUI.Models;
using ScriptKiddie.WinUI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.Mocks;

public class MockCourseSelectService : ICourseSelectService
{
    public Task AddCourseSelectPlan(CourseItem course, DateTime openTime, CancellationToken cancellationToken, int interval = 2000)
    {
        throw new NotImplementedException();
    }

    public Task<CourseResponse?> GetSelectableCoursesAsync()
    {
        throw new NotImplementedException();
    }

    public Task<List<CourseItem>?> GetSelectedCoursesAsync()
    {
        throw new NotImplementedException();
    }
}
