using Microsoft.UI.Xaml.Controls;
using ScriptKiddie.WinUI.Models;
using System.Collections.Generic;

namespace ScriptKiddie.WinUI.Pages.Controls;

public sealed partial class CourseToWithdrawSelectPanel : UserControl
{
    public readonly List<CourseItem> selectedCourses;

    public CourseToWithdrawSelectPanel(List<CourseItem> selectedCourses)
    {
        InitializeComponent();
        this.selectedCourses = selectedCourses;
    }
}
