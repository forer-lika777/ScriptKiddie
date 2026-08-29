using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ScriptKiddie.WinUI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.ViewModels;

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
