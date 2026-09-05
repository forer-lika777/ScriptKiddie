using CommunityToolkit.Mvvm.ComponentModel;
using ScriptKiddie.Core.Models;
using System.Collections.ObjectModel;

namespace ScriptKiddie.Core.ViewModels;

public partial class CourseToWithdrawSelectPanelModel : ObservableObject
{
    public CourseToWithdrawSelectPanelModel(ObservableCollection<CourseItem> selectedCourses)
    {
        this.SelectedCourses = selectedCourses;
        SelectedCourse = SelectedCourses[0];
    }

    [ObservableProperty]
    public partial ObservableCollection<CourseItem> SelectedCourses { get; set; } = [];

    [ObservableProperty]
    public partial CourseItem SelectedCourse { get; set; }
}
