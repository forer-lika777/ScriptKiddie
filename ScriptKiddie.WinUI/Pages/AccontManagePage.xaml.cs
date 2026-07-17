using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using ScriptKiddie.WinUI.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace ScriptKiddie.WinUI.Pages;

public sealed partial class AccountManagePage : Page
{
    public AccountManagePageModel ViewModel;

    public AccountManagePage()
    {
        InitializeComponent();
        ViewModel = App.Current.Services.GetRequiredService<AccountManagePageModel>();
    }
}
