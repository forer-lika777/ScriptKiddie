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

public class HttpClientProvider
{
    private HttpClient? httpClient;
    private HttpClientHandler? httpClientHandler;
    
    public HttpClientProvider()
    {
        CreateInstance();
    }

    private void CreateInstance(CookieCollection? cookies = null)
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

    public void SetCookies(CookieCollection? cookies = null)
    {
        CreateInstance(cookies);
    }

    public CookieCollection GetCookies()
    {
        return httpClientHandler!.CookieContainer.GetAllCookies();
    }

    public HttpClient GetCurrentClient()
    {
        return httpClient!;
    }
}
