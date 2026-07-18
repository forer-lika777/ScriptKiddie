using ScriptKiddie.WinUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.Services;

public interface ILoginService
{
    public Task<LoginResult> LoginAsync(LoginOption loginOption);
    public Task<bool> LogoutAsync();
}
