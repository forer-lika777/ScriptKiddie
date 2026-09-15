using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ScriptKiddie.Core.Models;
using ScriptKiddie.Core.Services;

namespace ScriptKiddie.Core.ViewModels;

public partial class MainWindowModel : ObservableObject, IRecipient<UpdateLoginStatusMessage>
{
    private readonly IAppSettingsService appSettingsService;
    private readonly IMessenger messenger;

    [ObservableProperty]
    public partial bool IsLoggedIn { get; set; } = false;

    public MainWindowModel(IAppSettingsService appSettingsService, IMessenger messenger)
    {
        this.appSettingsService = appSettingsService;
        this.messenger = messenger;

        messenger.Register<UpdateLoginStatusMessage>(this);

        IsLoggedIn = appSettingsService.IsLoggedIn.Value;
    }

    public void Receive(UpdateLoginStatusMessage message)
    {
        IsLoggedIn = message.IsLoggedIn;
    }
}
