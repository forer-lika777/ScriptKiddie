using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScriptKiddie.WinUI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.ViewModels;

public partial class LoginPageModel : ObservableObject
{
    private readonly AccountManageService accountManageService;

    public event EventHandler? LoginSucceeded;

    public LoginPageModel(AccountManageService accountManageService)
    {
        this.accountManageService = accountManageService;
    }

    [RelayCommand]
    private void Login()
    {
        LoginSucceeded?.Invoke(this, EventArgs.Empty);
    }


}
