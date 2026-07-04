using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Script_Kiddie.Models;

public class LoginOption
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; } = true;
    public bool LoadCookie { get; set; } = true;
    public bool ExportCookie { get; set; } = false;
    public string CookieContent { get; set; } = string.Empty;
    public Cookie? Cookie { get; set; } = null;
    public CookieLoadMode CookieLoadMode { get; set; } = CookieLoadMode.CookieString;
}
