using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ScriptKiddie.Core.Models;
using ScriptKiddie.Core.Services;

namespace ScriptKiddie.Core.ViewModels;

public partial class MainPageModel : ObservableObject, IRecipient<UpdateLoginStatusMessage>
{
    public MainPageModel(IAppSettingsService appSettingsService)
    {
        WeakReferenceMessenger.Default.Register<UpdateLoginStatusMessage>(this);

        IsLoggedIn = appSettingsService.IsLoggedIn.Value;
    }

    [ObservableProperty]
    public partial bool IsLoggedIn { get; set; } = false;

    public void Receive(UpdateLoginStatusMessage message)
    {
        IsLoggedIn = message.IsLoggedIn;
    }
}
