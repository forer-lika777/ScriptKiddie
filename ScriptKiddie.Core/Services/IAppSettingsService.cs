using ScriptKiddie.Core.Models;
using System.Collections.ObjectModel;

namespace ScriptKiddie.Core.Services;

public interface IAppSettingsService
{
    public IKeyItem<bool> IsLoggedIn { get; }
    public IKeyItem<AccountInfo> AccountInfo { get; }
    public IKeyItem<string> Password { get; }
    public IKeyItem<List<CookieItem>> Cookies { get; }
    public IKeyItem<ObservableCollection<SelectSchedule>> SelectSchedules { get; }
}

public interface IKeyItem<T> where T : notnull
{
    public string Name { get; }
    public T Value { get; set; }
    public void Load();
    public void Save();
}