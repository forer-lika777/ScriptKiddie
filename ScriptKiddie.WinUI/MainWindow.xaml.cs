using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using ScriptKiddie.Core.ViewModels;
using ScriptKiddie.WinUI.Views;
using ScriptKiddie.WinUI.Views.Pages;

namespace ScriptKiddie.WinUI;

public sealed partial class MainWindow : Window
{
    public MainWindowModel ViewModel { get; set; }

    public MainWindow()
    {
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;

        this.ViewModel = AppServices.GetRequiredService<MainWindowModel>();
    }
}
