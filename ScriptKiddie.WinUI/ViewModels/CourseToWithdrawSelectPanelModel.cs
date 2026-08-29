using CommunityToolkit.Mvvm.ComponentModel;
using ScriptKiddie.WinUI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.ViewModels;

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
