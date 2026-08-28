using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Enums.SettingsWindow;
using IslandCaller.TopmostEnhancer.ViewModels;

namespace IslandCaller.TopmostEnhancer.Views;

/// <summary>
/// 置顶增强设置页（纯代码构建，无 XAML 依赖）。
/// </summary>
[SettingsPageInfo("plugins.IslandCallerTopmostEnhancer", "IslandCaller 置顶增强", "\uE8B7", "\uE8B7", SettingsPageCategory.External)]
public partial class SettingsPage : SettingsPageBase
{
    private TextBlock? _savedTip;
    private readonly DispatcherTimer _hideSavedTip = new() { Interval = TimeSpan.FromSeconds(2) };

    public SettingsPage()
    {
        DataContext = new SettingsPageViewModel();
        _hideSavedTip.Tick += (_, _) =>
        {
            if (_savedTip is not null)
            {
                _savedTip.IsVisible = false;
            }

            _hideSavedTip.Stop();
        };
        BuildContent();
    }

    private void BuildContent()
    {
        var root = new StackPanel
        {
            Spacing = 6,
            Margin = new Thickness(8)
        };
        root.Classes.Add("settings-container");
        root.Classes.Add("animated-intro");

        // ---- 标题 ----
        root.Children.Add(new TextBlock
        {
            Text = "IslandCaller 置顶增强",
            FontSize = 17,
            FontWeight = FontWeight.SemiBold,
            Margin = new Thickness(0, 0, 0, 2)
        });
        root.Children.Add(new TextBlock
        {
            Text = "将 IslandCaller 悬浮窗与点名结果窗口的置顶优先级提升到最高级。",
            FontSize = 12,
            Opacity = 0.7,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 8)
        });

        // ---- 总开关 ----
        root.Children.Add(CreateToggle(
            "启用最高置顶增强",
            "关闭后停止所有置顶强化机制",
            nameof(Models.Settings.Enabled)));

        // ---- Z 序重推周期 ----
        var intervalRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Margin = new Thickness(0, 8, 0, 0)
        };
        intervalRow.Children.Add(new TextBlock
        {
            Text = "Z 序重推周期",
            VerticalAlignment = VerticalAlignment.Center,
            Width = 140
        });
        var slider = new Slider
        {
            Minimum = 50,
            Maximum = 2000,
            TickFrequency = 50,
            IsSnapToTickEnabled = true,
            Width = 220,
            VerticalAlignment = VerticalAlignment.Center
        };
        slider.Bind(Slider.ValueProperty, CreateBinding(nameof(Models.Settings.IntervalMs), BindingMode.TwoWay));
        intervalRow.Children.Add(slider);
        var intervalText = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = 60
        };
        intervalText.Bind(TextBlock.TextProperty, CreateBinding(nameof(Models.Settings.IntervalMs), BindingMode.OneWay, "{0} ms"));
        intervalRow.Children.Add(intervalText);
        root.Children.Add(intervalRow);
        root.Children.Add(new TextBlock
        {
            Text = "越小越“霸道”，与其它置顶窗口竞争时越占优（建议 100~500ms）",
            FontSize = 11,
            Opacity = 0.6,
            Margin = new Thickness(150, 0, 0, 0)
        });

        // ---- 强化机制 ----
        root.Children.Add(new TextBlock
        {
            Text = "强化机制",
            FontSize = 14,
            FontWeight = FontWeight.SemiBold,
            Margin = new Thickness(0, 10, 0, 2)
        });
        root.Children.Add(CreateToggle(
            "前台事件钩子",
            "任何窗口成为前台（切应用 / 全屏演示）时立即把 IslandCaller 窗口重新顶到最上",
            nameof(Models.Settings.EnableForegroundHook)));
        root.Children.Add(CreateToggle(
            "强制置顶样式",
            "持续写入 WS_EX_TOPMOST 扩展样式，确保窗口归属置顶带",
            nameof(Models.Settings.EnableTopmostStyle)));
        root.Children.Add(CreateToggle(
            "从 Alt+Tab 隐藏",
            "附加 WS_EX_TOOLWINDOW 样式，窗口不进入任务切换列表",
            nameof(Models.Settings.EnableToolWindow)));
        root.Children.Add(CreateToggle(
            "不抢焦点",
            "附加 WS_EX_NOACTIVATE 样式，置顶时不会夺取输入焦点",
            nameof(Models.Settings.EnableNoActivate)));
        root.Children.Add(CreateToggle(
            "UIA 增强检测",
            "识别被系统隐藏的 UWP / 现代化窗口（DWM cloaked），全屏与遮挡判定更准确",
            nameof(Models.Settings.EnableUiaDetection)));

        // ---- 额外关键词 ----
        root.Children.Add(new TextBlock
        {
            Text = "额外匹配的标题关键词",
            FontSize = 14,
            FontWeight = FontWeight.SemiBold,
            Margin = new Thickness(0, 10, 0, 2)
        });
        var keywordBox = new TextBox
        {
            PlaceholderText = "逗号分隔，例如：FluentShower, LiquidShower"
        };
        keywordBox.Bind(TextBox.TextProperty, CreateBinding(nameof(SettingsPageViewModel.ExtraTitleKeywordsText), BindingMode.TwoWay));
        root.Children.Add(keywordBox);
        root.Children.Add(new TextBlock
        {
            Text = "窗口标题包含任一关键词即纳入最高置顶守卫（默认已覆盖 IslandCaller 全部窗口）",
            FontSize = 11,
            Opacity = 0.6,
            TextWrapping = TextWrapping.Wrap
        });

        // ---- 保存更改 ----
        var saveRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Margin = new Thickness(0, 14, 0, 0)
        };
        var saveButton = new Button
        {
            Content = "保存更改",
            HorizontalAlignment = HorizontalAlignment.Left,
            Classes = { "accent" }
        };
        _savedTip = new TextBlock
        {
            Text = "已保存 ✓",
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = new SolidColorBrush(Color.Parse("#2E7D32")),
            IsVisible = false
        };
        saveButton.Click += (_, _) =>
        {
            // 手动立即写入配置文件（设置项本身也会在变更时自动保存，这里是显式落盘 + 反馈）
            Plugin.SaveSettings(((SettingsPageViewModel)DataContext!).Settings);
            if (_savedTip is not null)
            {
                _savedTip.IsVisible = true;
            }

            _hideSavedTip.Stop();
            _hideSavedTip.Start();
        };
        saveRow.Children.Add(saveButton);
        saveRow.Children.Add(_savedTip);
        root.Children.Add(saveRow);
        root.Children.Add(new TextBlock
        {
            Text = "设置项变更时会自动保存；点击此按钮可立即将所有更改写入配置文件。",
            FontSize = 11,
            Opacity = 0.6,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 4, 0, 0)
        });

        Content = new ScrollViewer { Content = root };
    }

    /// <summary>创建一个带标题与说明的开关行，绑定到 Settings 的属性。</summary>
    private Control CreateToggle(string header, string description, string settingsPropertyPath)
    {
        var textColumn = new StackPanel
        {
            Spacing = 2,
            VerticalAlignment = VerticalAlignment.Center
        };
        textColumn.Children.Add(new TextBlock
        {
            Text = header,
            FontSize = 14,
            FontWeight = FontWeight.SemiBold
        });
        textColumn.Children.Add(new TextBlock
        {
            Text = description,
            FontSize = 11,
            Opacity = 0.6,
            TextWrapping = TextWrapping.Wrap
        });

        var toggle = new ToggleSwitch
        {
            VerticalAlignment = VerticalAlignment.Center
        };
        // 绑定到 Settings.{property}：DataContext 为 SettingsPageViewModel
        var binding = new Binding
        {
            Path = $"Settings.{settingsPropertyPath}",
            Mode = BindingMode.TwoWay
        };
        toggle.Bind(ToggleSwitch.IsCheckedProperty, binding);

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            }
        };
        Grid.SetColumn(textColumn, 0);
        Grid.SetColumn(toggle, 1);
        grid.Children.Add(textColumn);
        grid.Children.Add(toggle);
        return grid;
    }

    private static Binding CreateBinding(string path, BindingMode mode, string? stringFormat = null)
    {
        return new Binding
        {
            Path = path,
            Mode = mode,
            StringFormat = stringFormat
        };
    }
}
