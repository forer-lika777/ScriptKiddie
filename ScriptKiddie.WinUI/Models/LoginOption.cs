using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ScriptKiddie.WinUI.Models;

public class LoginOption
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; } = true;
    public bool LoadCookie { get; set; } = true;
    public bool ExportCookie { get; set; } = false;
    public CookieCollection CookieContent { get; set; } = [];
}
