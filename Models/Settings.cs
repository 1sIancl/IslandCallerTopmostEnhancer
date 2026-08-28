using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace IslandCaller.TopmostEnhancer.Models;

/// <summary>
/// 置顶增强插件的配置。配置持久化在插件配置目录的 Settings.json 中，
/// 属性变更时自动保存（由 Plugin 入口类订阅 PropertyChanged）。
/// </summary>
public class Settings : INotifyPropertyChanged
{
    private bool _enabled = true;
    private int _intervalMs = 250;
    private bool _enableTopmostStyle = true;
    private bool _enableToolWindow = true;
    private bool _enableNoActivate = true;
    private bool _enableForegroundHook = true;
    private List<string> _extraTitleKeywords = new() { "FluentShower", "LiquidShower" };

    /// <summary>是否启用置顶增强（总开关）。</summary>
    public bool Enabled
    {
        get => _enabled;
        set => SetProperty(ref _enabled, value);
    }

    /// <summary>
    /// Z 序重推周期（毫秒）。越小越"霸道"（与其它置顶窗口竞争时越占优），
    /// 建议 100 ~ 500。IslandCaller 自带置顶循环为 3000ms。
    /// </summary>
    public int IntervalMs
    {
        get => _intervalMs;
        set => SetProperty(ref _intervalMs, value);
    }

    /// <summary>强化 WS_EX_TOPMOST 扩展样式（置顶带标记）。</summary>
    public bool EnableTopmostStyle
    {
        get => _enableTopmostStyle;
        set => SetProperty(ref _enableTopmostStyle, value);
    }

    /// <summary>附加 WS_EX_TOOLWINDOW 样式，从 Alt+Tab 任务切换中隐藏窗口。</summary>
    public bool EnableToolWindow
    {
        get => _enableToolWindow;
        set => SetProperty(ref _enableToolWindow, value);
    }

    /// <summary>附加 WS_EX_NOACTIVATE 样式，避免窗口抢焦点。</summary>
    public bool EnableNoActivate
    {
        get => _enableNoActivate;
        set => SetProperty(ref _enableNoActivate, value);
    }

    /// <summary>启用全局前台窗口事件钩子：一旦其它窗口成为前台，立即把 IslandCaller 窗口重新顶到最上。</summary>
    public bool EnableForegroundHook
    {
        get => _enableForegroundHook;
        set => SetProperty(ref _enableForegroundHook, value);
    }

    /// <summary>
    /// 额外匹配关键词（窗口标题包含任一关键词即视为目标窗口）。
    /// 默认覆盖 IslandCaller 的结果窗口标题。
    /// </summary>
    public List<string> ExtraTitleKeywords
    {
        get => _extraTitleKeywords;
        set => SetProperty(ref _extraTitleKeywords, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
