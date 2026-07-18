using Microsoft.UI.Xaml;
using ScriptKiddie.WinUI.Services;
using ScriptKiddie.WinUI.ViewModels;

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
