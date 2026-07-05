using System.Net;
using Script_Kiddie.Models;

namespace Script_Kiddie.Interfaces;

public interface ILoginService
{
    Task<LoginResult> LoginAsync(LoginOption option);
}

