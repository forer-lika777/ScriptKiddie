using ScriptKiddie.WinUI.Models;
using System.Threading;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.Services;

public interface ILoginService
{
    public Task<LoginResult> LoginAsync(LoginOption loginOption, CancellationToken cancellationToken);
    public Task<bool> LogoutAsync(CancellationToken cancellationToken);
    public string GetCaptchaImage();
    public string GetRandomCaptchaImage();
}
