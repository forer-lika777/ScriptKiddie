using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using ScriptKiddie.WinUI.Pages;
using ScriptKiddie.WinUI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.ViewModels;

public partial class MainWindowModel : ObservableObject, IRecipient<LoginSuccessMessage>
{
    private readonly AppSettingsService appSettingsService;

    [ObservableProperty]
    public partial bool IsLoggedIn { get; set; } = false;

    public MainWindowModel(AppSettingsService appSettingsService)
    {
        this.appSettingsService = appSettingsService;

        WeakReferenceMessenger.Default.Register(this);

        IsLoggedIn = appSettingsService.IsLoggedIn.Value;
    }

    public void Receive(LoginSuccessMessage message)
    {
        IsLoggedIn = true;
    }
}
