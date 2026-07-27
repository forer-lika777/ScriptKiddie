using Microsoft.UI.Xaml.Controls;
using ScriptKiddie.WinUI.Models;
using System.Collections.Generic;

namespace ScriptKiddie.WinUI.Pages.Controls;

public sealed partial class CourseInfoPanel : UserControl
{

    private List<PropertyDisplayItem> CoursePropertyList = [];

    public CourseInfoPanel(CourseItem courseItem)
    {
        InitializeComponent();
        CoursePropertyList.AddRange(courseItem.PropertyDisplayItems);
    }
}
