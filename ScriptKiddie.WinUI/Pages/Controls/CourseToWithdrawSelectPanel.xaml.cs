using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using ScriptKiddie.WinUI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

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
