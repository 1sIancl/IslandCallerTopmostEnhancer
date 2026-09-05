using ClassIsland.Core;
using ClassIsland.Core.Abstractions;
using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Extensions.Registry;
using ClassIsland.Core.Models.Automation;
using ClassIsland.Shared;
using Classcaller.Actions;
using Classcaller.Controls;
using Classcaller.Helpers;
using Classcaller.Models;
using Classcaller.Services;
using Classcaller.Services.ClasscallerService;
using Classcaller.Services.NotificationProvidersNew;
using Classcaller.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Classcaller
{
    [PluginEntrance]
    public class Plugin : PluginBase
    {
        public override void Initialize(HostBuilderContext context, IServiceCollection services)
        {
            var logger = IAppHost.TryGetService<ILogger<Plugin>>();
            services.AddSingleton<Status>();
            services.AddNotificationProvider<ClasscallerNotificationProviderNew>();
            services.AddSingleton<ClasscallerService>();
            services.AddSingleton<ProfileService>();
            services.AddSingleton<HistoryService>();
            services.AddSingleton<CoreService>();
            services.AddSingleton<ProfileRuntimeService>();
            services.AddSingleton<WindowsManager>();
            services.AddSingleton<LiquidGlassRuntime>();
            services.AddSingleton<WindowDragHelper>();
            services.AddSingleton<WindowSizeHelper>();
            services.AddSingleton<WindowTopmostHelper>();
            services.AddSingleton<ScreenBrightnessHelper>();
            services.AddSettingsPage<SettingPage>();
            BuildActionMenu();
            services.AddAction<DisableHoverAction>();
            services.AddAction<EnableHoverAction>();
            services.AddAction<CallAction>();
            services.AddAction<SwitchProfileAction, SwitchProfileActionSettingsControl>();
            AppBase.Current.AppStarted += async (_, _) =>
            {
                try
                {
                    logger = IAppHost.TryGetService<ILogger<Plugin>>();
                    IAppHost.GetService<Status>();
                    logger?.LogInformation("插件状态初始化完成，正在加载设置...");
                    new Settings(IAppHost.GetService<ProfileService>()).Load();
                    await IAppHost.GetService<LiquidGlassRuntime>().PrewarmAsync();
                    logger?.LogDebug("设置加载完成，正在加载默认配置...");
                    IAppHost.GetService<ProfileRuntimeService>().Initialize();
                    IAppHost.GetService<ClasscallerService>().Initialize();
                    IAppHost.GetService<WindowsManager>().Initialize();
                }
                catch (Exception ex)
                {
                    logger = IAppHost.GetService<ILogger<Plugin>>();
                    logger.LogCritical($"初始化失败：{ex}");
                    throw;
                }

            };
        }

        private static void BuildActionMenu()
        {
            IActionService.ActionMenuTree.Add(new ActionMenuTreeGroup("Classcaller 行动", "\uECF9"));
            IActionService.ActionMenuTree["Classcaller 行动"].Add(
                new ActionMenuTreeItem("Classcaller.Call", "随机点名", "\uECF9"));
            IActionService.ActionMenuTree["Classcaller 行动"].Add(
                new ActionMenuTreeItem("Classcaller.EnableHover", "启用悬浮窗", "\uF484"));
            IActionService.ActionMenuTree["Classcaller 行动"].Add(
                new ActionMenuTreeItem("Classcaller.DisableHover", "禁用悬浮窗", "\uF486"));
            IActionService.ActionMenuTree["Classcaller 行动"].Add(
                new ActionMenuTreeItem("Classcaller.SwitchProfile", "切换档案", "\uE9A8"));
        }
    }
}
