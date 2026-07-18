using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ScriptKiddie.WinUI.Models;

public class LoginResult
{
    public bool Success { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string ResponseContent { get; set; } = string.Empty;
    public CookieCollection CookieContent { get; set; } = [];
    public string AccountName { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;
}
