using Microsoft.Extensions.Logging;
using ScriptKiddie.WinUI.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.DataProtection;
using Windows.Storage;
using Windows.Storage.Streams;

namespace ScriptKiddie.WinUI.Services;

public class AppSettingsService
{
    private readonly ILogger<AppSettingsService> logger;

    public AppSettingsService(ILogger<AppSettingsService> logger)
    {
        this.logger = logger;
    }

    public KeyItem<bool> IsLoggedIn { get; } = new(nameof(IsLoggedIn), false);
    public KeyItem<AccountInfo> AccountInfo { get; } = new(nameof(AccountInfo), new AccountInfo());
    public SecureKeyItem<List<CookieItem>> Cookies { get; } = new(nameof(Cookies), []);

    public class KeyItem<T> where T : notnull
    {
        public KeyItem(string name, T defaultValue)
        {
            Name = name;
            value = defaultValue;
            Load();
        }

        public string Name { get; }
        public T Value
        {
            get => value;
            set
            {
                if (EqualityComparer<T>.Default.Equals(this.value, value)) return;
                this.value = value;
                Save();
            }
        }

        private T value;

        public void Load()
        {
            try
            {
                var raw = ApplicationData.Current.LocalSettings.Values[Name];

                if (raw == null) return;

                if (isDirectlySupported && raw is T typedValue)
                {
                    value = typedValue;
                    return;
                }

                if (raw is string json)
                {
                    var deserialized = JsonSerializer.Deserialize<T>(json);
                    if (deserialized is not null)
                    {
                        value = deserialized;
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"加载设置 '{Name}' 失败。错误信息: {ex.Message}");
            }
        }

        public void Save()
        {
            try
            {
                object valueToStore;

                if (isDirectlySupported)
                {
                    valueToStore = value!;
                }
                else
                {
                    valueToStore = JsonSerializer.Serialize(value);
                }

                ApplicationData.Current.LocalSettings.Values[Name] = valueToStore;
            }
            catch (JsonException jsonEx)
            {
                Debug.WriteLine($"保存设置 '{Name}' 失败。检查是否为此类添加了 JSON 序列化注解？类型：{typeof(T)}，序列化错误信息: {jsonEx.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"保存设置 '{Name}' 失败。错误信息: {ex.Message}");
                throw;
            }
        }

        private static readonly bool isDirectlySupported =
            typeof(T) == typeof(bool) ||
            typeof(T) == typeof(string) ||
            typeof(T) == typeof(int) ||
            typeof(T) == typeof(uint) ||
            typeof(T) == typeof(long) ||
            typeof(T) == typeof(ulong) ||
            typeof(T) == typeof(float) ||
            typeof(T) == typeof(double) ||
            typeof(T) == typeof(char) ||
            typeof(T) == typeof(DateTime) ||
            typeof(T) == typeof(TimeSpan) ||
            typeof(T) == typeof(Guid) ||
            typeof(T) == typeof(byte[]);
    }

    public class SecureKeyItem<T> where T : notnull
    {
        public SecureKeyItem(string name, T defaultValue)
        {
            Name = name;
            value = defaultValue;
            Load();
        }

        public string Name { get; }
        public T Value
        {
            get => value;
            set
            {
                if (EqualityComparer<T>.Default.Equals(this.value, value)) return;
                this.value = value;
                Save();
            }
        }

        private T value;

        public void Load()
        {
            try
            {
                var raw = ApplicationData.Current.LocalSettings.Values[Name];

                if (raw == null) return;

                if (raw is string data)
                {
                    var deserialized = JsonSerializer.Deserialize<T>(SecureKeyItem<T>.Decrypt(data));
                    if (deserialized is not null)
                    {
                        value = deserialized;
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"加载设置 '{Name}' 失败。错误信息: {ex.Message}");
            }
        }

        public void Save()
        {
            try
            {
                string jsonData = JsonSerializer.Serialize(value);

                ApplicationData.Current.LocalSettings.Values[Name] = SecureKeyItem<T>.Encrypt(jsonData);
            }
            catch (JsonException jsonEx)
            {
                Debug.WriteLine($"保存设置 '{Name}' 失败。检查是否为此类添加了 JSON 序列化注解？类型：{typeof(T)}，序列化错误信息: {jsonEx.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"保存加密设置失败。设置名: '{Name}'。错误: {ex.Message}");
                throw;
            }
        }

        private static string Encrypt(string data)
        {
            if (string.IsNullOrEmpty(data)) return string.Empty;

            // "LOCAL=user" 表示该加密数据仅限当前 Windows 登录用户解密
            var provider = new DataProtectionProvider("LOCAL=user");
            IBuffer contentBuffer = CryptographicBuffer.ConvertStringToBinary(data, BinaryStringEncoding.Utf8);

            // 异步转同步获取结果
            IBuffer protectedBuffer = provider.ProtectAsync(contentBuffer).AsTask().GetAwaiter().GetResult();

            // 转为 Base64 字符串保存到 LocalSettings
            return CryptographicBuffer.EncodeToBase64String(protectedBuffer);
        }

        private static string Decrypt(string data) 
        {
            if (string.IsNullOrEmpty(data)) return string.Empty;

            try
            {
                var provider = new DataProtectionProvider();
                IBuffer protectedBuffer = CryptographicBuffer.DecodeFromBase64String(data);

                IBuffer clearBuffer = provider.UnprotectAsync(protectedBuffer).AsTask().GetAwaiter().GetResult();
                return CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, clearBuffer);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"解密失败，数据可能损坏或密钥不匹配: {ex.Message}");
                return JsonSerializer.Serialize(default(T)!); // 返回默认值的 JSON 字符串防止反序列化崩溃
            }
        }
    }
}