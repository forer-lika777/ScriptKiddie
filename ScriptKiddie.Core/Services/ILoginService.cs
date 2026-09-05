using ScriptKiddie.Core.Models;

namespace ScriptKiddie.Core.Services;

public interface ILoginService
{
    public Task<LoginResult> LoginAsync(LoginOption loginOption, CancellationToken cancellationToken);
    public Task<bool> LogoutAsync(CancellationToken cancellationToken);
    public string GetCaptchaImage();
    public string GetRandomCaptchaImage();
}
