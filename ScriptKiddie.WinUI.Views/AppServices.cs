using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptKiddie.WinUI.Views;

/// <summary>壳启动时把 DI 容器注入到这里，Views 层统一从这取服务。</summary>
public static class AppServices
{
    private static IServiceProvider? provider;

    public static void Initialize(IServiceProvider provider) => AppServices.provider = provider;

    public static T GetRequiredService<T>() where T : notnull
    {
        if (provider is null)
            throw new NullReferenceException("尚未注入 DI 容器！");

        return provider.GetRequiredService<T>();
    }
}
