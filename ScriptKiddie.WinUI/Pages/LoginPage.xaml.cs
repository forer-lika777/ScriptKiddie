using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ScriptKiddie.WinUI.ViewModels;
using Windows.System;

namespace ScriptKiddie.WinUI.Pages;

public sealed partial class LoginPage : Page
{
    public LoginPageModel ViewModel { get; set; }

    public LoginPage()
    {
        InitializeComponent();
        ViewModel = App.Current.Services.GetRequiredService<LoginPageModel>();

        TextBox textBox = new TextBox();
    }

    private void UserNameTextBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter)
        {
            PasswordBox.Focus(FocusState.Programmatic);
            e.Handled = true;
        }
    }

    private void PasswordBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter)
        {
            // 如果需要验证码，且验证码输入框正在显示，跳转到验证码框
            if (ViewModel.NeedCaptcha)
            {
                CaptchaTextBox.Focus(FocusState.Programmatic);
            }
            else
            {
                // 不需要验证码且按钮可点击时，直接触发登录
                if (ViewModel.LoginCommand.CanExecute(null))
                {
                    ViewModel.LoginCommand.Execute(null);
                }
            }
            e.Handled = true;
        }
    }

    private void CaptchaTextBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter)
        {
            if (ViewModel.LoginCommand.CanExecute(null))
            {
                ViewModel.LoginCommand.Execute(null);
            }
            e.Handled = true;
        }
    }
}
