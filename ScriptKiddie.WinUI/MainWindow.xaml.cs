using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using ScriptKiddie.WinUI.Pages;
using ScriptKiddie.WinUI.Services;
using ScriptKiddie.WinUI.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace ScriptKiddie.WinUI;

public sealed partial class MainWindow : Window
{
    private readonly AppSettingsService appSettingsService;
    public MainWindowModel ViewModel { get; }

    public MainWindow(AppSettingsService appSettingsService, MainWindowModel mainWindowModel)
    {
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        
        this.appSettingsService = appSettingsService;
        ViewModel = mainWindowModel;
    }
}
