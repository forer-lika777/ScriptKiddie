using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using ScriptKiddie.WinUI.Pages;
using ScriptKiddie.WinUI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.ViewModels;

public partial class MainWindowModel : ObservableObject
{
    private readonly AppSettingsService appSettingsService;

    [ObservableProperty]
    public partial bool IsLoggedIn { get; set; } = false;

    public MainWindowModel(AppSettingsService appSettingsService)
    {
        this.appSettingsService = appSettingsService;
        Initialize();
    }

    private void Initialize()
    {
        if (!appSettingsService.IsLoggedIn.Value)
        {
            var loginViewModel = App.Current.Services.GetRequiredService<LoginPageModel>();
            loginViewModel.LoginSucceeded += OnLoginSucceeded;

            IsLoggedIn = false;
        }
        else
        {
            IsLoggedIn = true;
        }
    }

    private void OnLoginSucceeded(object? sender, EventArgs e)
    {
        //if (sender is LoginPageModel loginViewModel)
        //{
        //    loginViewModel.LoginSucceeded -= OnLoginSucceeded;
        //}

        IsLoggedIn = true;
    }
}
