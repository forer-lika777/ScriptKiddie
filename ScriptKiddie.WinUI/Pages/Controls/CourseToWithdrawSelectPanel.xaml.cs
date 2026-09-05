using Microsoft.UI.Xaml.Controls;
using ScriptKiddie.Core.Models;
using ScriptKiddie.Core.ViewModels;
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
