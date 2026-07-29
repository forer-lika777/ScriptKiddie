using ScriptKiddie.WinUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.Services;

public interface IAppSettingsService
{
    public IKeyItem<bool> IsLoggedIn { get; }
    public IKeyItem<AccountInfo> AccountInfo { get; }
    public IKeyItem<string> Password { get; }
    public IKeyItem<List<CookieItem>> Cookies { get; }
}

public interface IKeyItem<T> where T : notnull
{
    public string Name { get; }
    public T Value { get; set; }
    public void Load();
    public void Save();
}