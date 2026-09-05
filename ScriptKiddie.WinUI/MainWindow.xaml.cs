using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using ScriptKiddie.Core.ViewModels;

namespace ScriptKiddie.WinUI;

public sealed partial class MainWindow : Window
{
    public MainWindowModel ViewModel { get; set; }

    public MainWindow()
    {
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;

        this.ViewModel = App.Current.Services.GetRequiredService<MainWindowModel>();
    }
}
