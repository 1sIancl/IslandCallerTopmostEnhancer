using System;
using System.IO;
using ClassIsland.Core;
using ClassIsland.Core.Abstractions;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Extensions.Registry;
using ClassIsland.Shared;
using ClassIsland.Shared.Helpers;
using IslandCaller.TopmostEnhancer.Models;
using IslandCaller.TopmostEnhancer.Services;
using IslandCaller.TopmostEnhancer.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IslandCaller.TopmostEnhancer;

/// <summary>
/// IslandCaller 置顶增强插件入口。
///
/// 插件本身不依赖 IslandCaller 的任何程序集（跨插件程序集隔离），
/// 运行时通过反射 / Win32 识别 IslandCaller 的窗口并执行最高级置顶。
/// </summary>
[PluginEntrance]
public class Plugin : PluginBase
{
    private TopmostEnhancerService? _topmostEnhancerService;

    /// <summary>插件配置（持久化在插件配置目录 Settings.json）。</summary>
    public Settings Settings { get; set; } = new();

    public override void Initialize(HostBuilderContext context, IServiceCollection services)
    {
        // 加载 / 保存配置
        var settingsPath = Path.Combine(PluginConfigFolder, "Settings.json");
        Settings = ConfigureFileHelper.LoadConfig<Settings>(settingsPath);
        Settings.PropertyChanged += (_, _) =>
            ConfigureFileHelper.SaveConfig(settingsPath, Settings);

        // 注册服务与设置页
        services.AddSingleton(Settings);
        services.AddSingleton<TopmostEnhancerService>();
        services.AddSettingsPage<SettingsPage>();

        // 应用启动完成后启动置顶增强（此时 Avalonia Application 已就绪）
        AppBase.Current.AppStarted += (_, _) =>
        {
            try
            {
                _topmostEnhancerService = IAppHost.GetService<TopmostEnhancerService>();
                _topmostEnhancerService.Start();
            }
            catch (Exception ex)
            {
                IAppHost.GetService<ILogger<Plugin>>()
                    .LogCritical(ex, "IslandCaller 置顶增强服务启动失败。");
            }
        };

        // 应用退出前释放钩子与定时器
        AppBase.Current.AppStopping += (_, _) =>
        {
            _topmostEnhancerService?.Dispose();
            _topmostEnhancerService = null;
        };
    }
}
