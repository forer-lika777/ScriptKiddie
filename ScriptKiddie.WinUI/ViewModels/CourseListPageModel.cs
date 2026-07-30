using CommunityToolkit.Mvvm.ComponentModel;
using ScriptKiddie.WinUI.Models;
using ScriptKiddie.WinUI.Services;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.ViewModels;

public partial class CourseListPageModel : ObservableObject
{
    private readonly AccountManageService accountManageService;
    public CourseListPageModel(ICourseSelectService courseSelectService, AccountManageService accountManageService)
    {
        this.accountManageService = accountManageService;
        _ = SyncCoursesContent();
    }

    private async Task SyncCoursesContent()
    {
        CourseResponse? selectableCourse = await accountManageService.GetSelectableCoursesAsync();
        List<CourseItem>? selectedCourses = await accountManageService.GetSelectedCoursesAsync();
        int? limitCount = await accountManageService.GetSelectLimitCountAsync();
        if (selectableCourse is null || selectedCourses is null || limitCount is null)
        {
            LoadingFailed = true;
        }
        else
        {
            SelectableCourses = selectableCourse.Rows;
            SelectedCourses = selectedCourses;
            SelectLimitCount = (int)limitCount;
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

    [ObservableProperty]
    public partial List<CourseItem> PreSelectCourses { get; set; } = [];

    [ObservableProperty]
    public partial int SelectLimitCount { get; set; } = 0;
}
