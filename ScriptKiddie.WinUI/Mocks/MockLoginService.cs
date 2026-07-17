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
        await Task.Delay(1000);

        return new LoginResult
        {
            Success = true,
            Grade = "10086",
            AccountName = "脚本小子",
            StatusCode = HttpStatusCode.OK,
            Message = "你好脚本小子",
            ResponseContent = "。",
        };
    }
}
