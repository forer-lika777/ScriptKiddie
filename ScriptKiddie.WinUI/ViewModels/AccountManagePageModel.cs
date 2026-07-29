using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using ScriptKiddie.WinUI.Models;
using ScriptKiddie.WinUI.Services;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.ViewModels;

public partial class AccountManagePageModel : ObservableObject, IRecipient<AccountInfoChangedMessage>
{
    private readonly AccountManageService accountManageService;
    private readonly IAppSettingsService appSettingsService;

    public AccountManagePageModel(AccountManageService accountManageService, IAppSettingsService appSettingsService)
    {
        this.accountManageService = accountManageService;
        this.appSettingsService = appSettingsService;

        WeakReferenceMessenger.Default.Register(this);

        RefreshAccountInfo(accountManageService.GetAccountInfo());
    }

    [ObservableProperty]
    public partial string AccountName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AccountId { get; set; } = string.Empty;

    public void Receive(AccountInfoChangedMessage message)
    {
        RefreshAccountInfo(message.value);
    }

    private void RefreshAccountInfo(AccountInfo? accountInfo)
    {
        if (accountInfo is null)
            return;
        AccountName = accountInfo.AccountName;
        AccountId = accountInfo.AccountId;
    }

    [RelayCommand]
    private async Task ExitLogin()
    {
        if (await accountManageService.LogoutAsync())
        {
            appSettingsService.IsLoggedIn.Value = false;
            var mainWindowModel = App.Current.Services.GetRequiredService<MainWindowModel>();
            mainWindowModel.IsLoggedIn = false;
        }
    }
}
