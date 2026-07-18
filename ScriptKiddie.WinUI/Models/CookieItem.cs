using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json.Serialization;

namespace ScriptKiddie.WinUI.Models;

public class CookieItem
{
    [JsonPropertyName(nameof(Name))]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName(nameof(Value))]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName(nameof(Domain))]
    public string? Domain { get; set; }

    [JsonPropertyName(nameof(Path))]
    public string? Path { get; set; }

    [JsonPropertyName(nameof(Expires))]
    public DateTime? Expires { get; set; }

    [JsonPropertyName(nameof(Secure))]
    public bool Secure { get; set; }

    [JsonPropertyName(nameof(HttpOnly))]
    public bool HttpOnly { get; set; }

    [JsonPropertyName(nameof(SameSite))]
    public string? SameSite { get; set; }

    public override string ToString()
    {
        return $"{Name}={Value}";
    }

    // 转成请求头用的字符串
    public string ToHeaderString()
    {
        return $"{Name}={Value}";
    }
}

/// <summary>
/// CookieCollection 与 List&lt;CookieItem&gt; 互转的扩展方法。
/// </summary>
public static class CookieCollectionExtensions
{
    /// <summary>
    /// 将 <see cref="CookieCollection"/> 转换为 <see cref="List{CookieItem}"/>。
    /// </summary>
    /// <param name="collection">要转换的 Cookie 集合。</param>
    /// <returns>转换后的 <see cref="List{CookieItem}"/> 列表。</returns>
    public static List<CookieItem> ToCookieItemList(this CookieCollection collection)
    {
        var result = new List<CookieItem>(collection.Count);
        foreach (Cookie cookie in collection)
        {
            result.Add(new CookieItem
            {
                Name = cookie.Name,
                Value = cookie.Value,
                Domain = cookie.Domain,
                Path = cookie.Path,
                Expires = cookie.Expires == DateTime.MinValue ? null : cookie.Expires,
                Secure = cookie.Secure,
                HttpOnly = cookie.HttpOnly,
            });
        }
        return result;
    }

    /// <summary>
    /// 将 <see cref="List{CookieItem}"/> 转换回 <see cref="CookieCollection"/>。
    /// </summary>
    /// <param name="items">要转换的 CookieItem 列表。</param>
    /// <returns>转换后的 <see cref="CookieCollection"/> 集合。</returns>
    public static CookieCollection ToCookieCollection(this List<CookieItem> items)
    {
        var collection = new CookieCollection();
        foreach (var item in items)
        {
            var cookie = new Cookie
            {
                Name = item.Name,
                Value = item.Value,
                Domain = item.Domain ?? string.Empty,
                Path = item.Path ?? "/",
                Secure = item.Secure,
                HttpOnly = item.HttpOnly,
            };
            if (item.Expires.HasValue)
            {
                cookie.Expires = item.Expires.Value;
            }
            collection.Add(cookie);
        }
        return collection;
    }
}