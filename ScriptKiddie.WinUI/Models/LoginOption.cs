using System.Net;

namespace ScriptKiddie.WinUI.Models;

public class LoginOption
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Captcha { get; set; } = string.Empty;
    public bool RememberMe { get; set; } = true;
    public bool LoadCookie { get; set; } = true;
    public bool ExportCookie { get; set; } = false;
    public CookieCollection? CookieContent { get; set; } = null;
}
