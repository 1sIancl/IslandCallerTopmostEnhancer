using System.Collections.Generic;
using System.Linq;
using ClassIsland.Shared;
using IslandCaller.TopmostEnhancerNet8.Models;

namespace IslandCaller.TopmostEnhancerNet8.ViewModels;

/// <summary>
/// 置顶增强设置页视图模型。设置直接读写 <see cref="Settings"/>（自动持久化）。
/// </summary>
public class SettingsPageViewModel
{
    /// <summary>插件设置（由 ClassIsland 依赖注入提供单例）。</summary>
    public Settings Settings { get; }

    public SettingsPageViewModel()
    {
        Settings = IAppHost.GetService<Settings>();
    }

    /// <summary>
    /// 额外匹配关键词的文本形式（逗号/分号分隔），用于设置页 TextBox 双向绑定。
    /// </summary>
    public string ExtraTitleKeywordsText
    {
        get => string.Join(", ", Settings.ExtraTitleKeywords);
        set
        {
            Settings.ExtraTitleKeywords = (value ?? string.Empty)
                .Split(',', '，', ';', '；')
                .Select(k => k.Trim())
                .Where(k => k.Length > 0)
                .Distinct()
                .ToList();
        }
    }
}
