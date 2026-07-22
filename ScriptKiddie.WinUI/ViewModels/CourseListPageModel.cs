using CommunityToolkit.Mvvm.ComponentModel;
using ScriptKiddie.WinUI.Models;
using ScriptKiddie.WinUI.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.ViewModels;

public partial class CourseListPageModel : ObservableObject
{
    private readonly ICourseSelectService courseSelectService;
    public CourseListPageModel(ICourseSelectService courseSelectService)
    {
        this.courseSelectService = courseSelectService;
        _ = SyncCoursesContent();
    }

    private async Task SyncCoursesContent()
    {
        var selectableCourse = await courseSelectService.GetSelectableCoursesAsync();
        var selectedCourses = await courseSelectService.GetSelectedCoursesAsync();
        if (selectableCourse is null || selectedCourses is null)
        {
            LoadingFailed = true;
        }
        else
        {
            SelectableCourses = selectableCourse.Rows;
            SelectedCourses = selectedCourses;
        }
        IsLoading = false;
    }

    [ObservableProperty]
    public partial bool IsLoggedIn { get; set; } = false;

    [ObservableProperty]
    public partial bool IsLoading { get; set; } = true;

    [ObservableProperty]
    public partial bool LoadingFailed { get; set; } = false;

    [ObservableProperty]
    public partial List<CourseItem> SelectableCourses { get; set; } = [];

    [ObservableProperty]
    public partial List<CourseItem> SelectedCourses { get; set; } = [];
}
