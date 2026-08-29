using Microsoft.UI.Xaml.Controls;
using ScriptKiddie.WinUI.Models;
using ScriptKiddie.WinUI.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ScriptKiddie.WinUI.Pages.Controls;

public sealed partial class CourseToWithdrawSelectPanel : UserControl
{
    public CourseToWithdrawSelectPanelModel ViewModel { get; set; }

    public CourseToWithdrawSelectPanel(ObservableCollection<CourseItem> selectedCourses)
    {
        InitializeComponent();
        ViewModel = new CourseToWithdrawSelectPanelModel(selectedCourses);
    }
}
