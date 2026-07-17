using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace ScriptKiddie.WinUI.Models
{
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
}