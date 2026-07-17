using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.ViewModels;

partial class CourseListPageModel : ObservableObject
{
    public CourseListPageModel()
    {
        
    }

    [ObservableProperty]
    public partial bool IsLoggedIn { get; set; } = false;


}
