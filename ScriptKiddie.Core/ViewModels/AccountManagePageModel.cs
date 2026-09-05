using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ScriptKiddie.Core.Models;
using ScriptKiddie.Core.Services;

namespace ScriptKiddie.Core.ViewModels;

public partial class AccountManagePageModel : ObservableObject, IRecipient<AccountInfoChangedMessage>
{
    private readonly IAccountManageService accountManageService;
    private readonly IAppSettingsService appSettingsService;

    public AccountManagePageModel(IAccountManageService accountManageService, IAppSettingsService appSettingsService)
    {
        this.accountManageService = accountManageService;
        this.appSettingsService = appSettingsService;

        WeakReferenceMessenger.Default.Register<AccountInfoChangedMessage>(this);

        RefreshAccountInfo(accountManageService.GetAccountInfo());
    }

    [ObservableProperty]
    public partial string AccountName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AccountId { get; set; } = string.Empty;

    public void Receive(AccountInfoChangedMessage message)
    {
        RefreshAccountInfo(message.Value);
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
            WeakReferenceMessenger.Default.Send<UpdateLoginStatusMessage>(new UpdateLoginStatusMessage(false));
        }
    }
}
