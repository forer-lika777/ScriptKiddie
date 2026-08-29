using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ScriptKiddie.WinUI.Services;

namespace ScriptKiddie.WinUI.ViewModels;

public partial class MainWindowModel : ObservableObject, IRecipient<UpdateLoginStatusMessage>
{
    private readonly IAppSettingsService appSettingsService;

    [ObservableProperty]
    public partial bool IsLoggedIn { get; set; } = false;

    public MainWindowModel(IAppSettingsService appSettingsService)
    {
        this.appSettingsService = appSettingsService;

        WeakReferenceMessenger.Default.Register<UpdateLoginStatusMessage>(this);

        IsLoggedIn = appSettingsService.IsLoggedIn.Value;
    }

    public void Receive(UpdateLoginStatusMessage message)
    {
        IsLoggedIn = message.IsLoggedIn;
    }
}
