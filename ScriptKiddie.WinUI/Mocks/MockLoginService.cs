using ScriptKiddie.WinUI.Models;
using ScriptKiddie.WinUI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.Mocks;

public class MockLoginService : ILoginService
{
    public async Task<LoginResult> LoginAsync(LoginOption loginOption)
    {
        await Task.Delay(2000);

        return new LoginResult
        {
            Success = true,
            Grade = "80808080",
            AccountName = "WinUI 的受害者之一。",
            StatusCode = HttpStatusCode.OK,
            Message = "。",
            ResponseContent = ".",
        };
    }

    public async Task<bool> LogoutAsync()
    {
        await Task.Delay(1000);
        return true;
    }
}
