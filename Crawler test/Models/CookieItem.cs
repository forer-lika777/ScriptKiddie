using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Script_Kiddie.Models;

public class CookieItem
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("domain")]
    public string? Domain { get; set; }

    [JsonPropertyName("path")]
    public string? Path { get; set; }

    [JsonPropertyName("expires")]
    public DateTime? Expires { get; set; }

    [JsonPropertyName("secure")]
    public bool Secure { get; set; }

    [JsonPropertyName("httpOnly")]
    public bool HttpOnly { get; set; }

    [JsonPropertyName("sameSite")]
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