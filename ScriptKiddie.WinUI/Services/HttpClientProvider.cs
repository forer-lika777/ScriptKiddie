using Microsoft.UI.Xaml.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.Services;

/// <summary>
/// 提供全局唯一的 HttpClient 实例，支持线程安全的 Cookie 管理。
/// </summary>
public class HttpClientProvider
{
    private readonly ReaderWriterLockSlim rwLock = new();
    private HttpClient? httpClient;
    private HttpClientHandler? httpClientHandler;
    
    public HttpClientProvider()
    {
        CreateInstance();
    }

    private void CreateInstance(CookieCollection? cookies = null)
    {
        rwLock.EnterWriteLock();
        try
        {
            httpClient?.Dispose();
            httpClientHandler?.Dispose();

            httpClientHandler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                UseCookies = true,
                CookieContainer = new CookieContainer(),
            };

            httpClient = new HttpClient(httpClientHandler);
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
            
            if (cookies != null)
            {
                httpClientHandler.CookieContainer.Add(cookies);
            }
        }
        finally
        {
            rwLock.ExitWriteLock();
        }
    }

    public void SetCookies(CookieCollection? cookies = null)
    {
        CreateInstance(cookies);
    }

    public CookieCollection GetCookies()
    {
        rwLock.EnterReadLock();
        try
        {
            return httpClientHandler!.CookieContainer.GetAllCookies();
        }
        finally
        {
            rwLock.ExitReadLock();
        }
    }

    public HttpClient GetCurrentClient()
    {
        rwLock.EnterReadLock();
        try
        {
            return httpClient!;
        }
        finally
        {
            rwLock.ExitReadLock();
        }
    }
}
