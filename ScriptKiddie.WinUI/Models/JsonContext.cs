using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json.Serialization;

namespace ScriptKiddie.WinUI.Models;

[JsonSerializable(typeof(CourseResponse))]
[JsonSerializable(typeof(List<CourseItem>))]
[JsonSerializable(typeof(CourseItem))]
public partial class CourseResponseJsonContext : JsonSerializerContext
{

}

[JsonSerializable(typeof(List<CourseItem>))]
public partial class CourseItemListJsonContext : JsonSerializerContext
{

}

[JsonSerializable(typeof(List<CookieItem>))]
[JsonSerializable(typeof(CookieItem))]
public partial class CookieJsonContext : JsonSerializerContext
{

}

[JsonSerializable(typeof(AccountInfo))]
public partial class AccountInfoJsonContext : JsonSerializerContext
{

}

[JsonSerializable(typeof(ObservableCollection<SelectSchedule>))]
public partial class SelectScheduleListContext : JsonSerializerContext
{

}

[JsonSerializable(typeof(CourseItem))]
[JsonSerializable(typeof(List<CourseItem>))]
[JsonSerializable(typeof(Dictionary<string, object>))]
[JsonSerializable(typeof(ObservableCollection<SelectSchedule>))]
public partial class AppJsonContext : JsonSerializerContext
{

}