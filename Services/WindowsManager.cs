using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using ClassIsland.Core.Controls;
using Classcaller.Helpers;
using Classcaller.Models;
using Classcaller.Views;
using Microsoft.Extensions.Logging;

namespace Classcaller.Services;

internal class WindowsManager
{
    private readonly ILogger<WindowsManager> _logger;
    private readonly ScreenBrightnessHelper _screenBrightnessHelper;
    private readonly LiquidGlassRuntime _liquidGlassRuntime;
    private bool _isInitialized;
    private bool _isRecreatingHover;

    public Window? HoverWindow { get; private set; }
    public Window? ShowerWindow { get; private set; }

    public WindowsManager(
        ILogger<WindowsManager> logger,
        ScreenBrightnessHelper screenBrightnessHelper,
        LiquidGlassRuntime liquidGlassRuntime)
    {
        _logger = logger;
        _screenBrightnessHelper = screenBrightnessHelper;
        _liquidGlassRuntime = liquidGlassRuntime;
        _logger.LogTrace("WindowsManager created.");
    }

    internal void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;
        Settings.Instance.Hover.PropertyChanged += OnHoverSettingChanged;
        if (Settings.Instance.Hover.IsEnable)
        {
            ShowHoverWindow();
        }

        _logger.LogInformation("WindowsManager initialized.");
    }

    internal async Task ShowCallWindowAsync(string text, float duration, CancellationToken token)
    {
        _logger.LogInformation("Showing call window for {Duration} seconds with text: {Text}", duration, text);
        var appearance = Settings.Instance.Appearance;
        var icon = CreateResultIcon(appearance);
        var nameText = new TextBlock
        {
            Text = text,
            FontSize = appearance.ResultFontSize,
            FontWeight = FontWeight.Bold,
            FontStretch = FontStretch.Expanded,
            FontFamily = string.IsNullOrWhiteSpace(appearance.FontFamily) ? null : new FontFamily(appearance.FontFamily),
            Margin = new Thickness(15, 0, 0, 0),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
        var showPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Thickness(25, 0),
            Children = { icon, nameText }
        };

        // 复用展示窗口：液态玻璃窗口反复创建/销毁会导致 MorerialsAvalonia
        // 的 Desktop Duplication / D3D11 资源无法及时释放，内存持续增长。
        // 因此只创建一次，点名时更新内容并显示，结束后隐藏而非销毁。
        var showerWindow = GetOrCreateShowerWindow();

        if (showerWindow is LiquidShower liquidShower)
        {
            liquidShower.SetDisplayContent(showPanel);
        }
        else
        {
            showerWindow.Content = showPanel;
        }

        showPanel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        var screen = showerWindow.Screens.Primary;
        PixelRect? captureRect = null;
        if (screen is not null && showPanel.DesiredSize.Width > 0)
        {
            var scaling = screen.Scaling;
            var width = Math.Max(1, (int)Math.Ceiling(showPanel.DesiredSize.Width));
            // 高度随结果字号自适应，避免大字号时被固定 110 高度裁剪
            var height = Math.Max(110, (int)Math.Ceiling(showPanel.DesiredSize.Height + 40));
            showerWindow.Width = width;
            showerWindow.Height = height;

            if (showerWindow is LiquidShower liquidGlassWindow)
            {
                liquidGlassWindow.ApplyGlassExtent(height);
            }

            var widthPixels = Math.Max(1, (int)Math.Ceiling(width * scaling));
            var heightPixels = Math.Max(1, (int)Math.Ceiling(height * scaling));
            var workArea = screen.WorkingArea;
            var x = workArea.X + Math.Max(0, (workArea.Width - widthPixels) / 2);
            var y = workArea.Y + Math.Max(0, (workArea.Height - heightPixels) / 2);
            showerWindow.Position = new PixelPoint(x, y);
            captureRect = new PixelRect(x, y, widthPixels, heightPixels);
        }

        if (showerWindow is FluentShower)
        {
            // 自定义结果文字色优先；留空则按屏幕亮度自动选黑白
            var foreground = ParseBrush(appearance.ResultTextColor);
            if (foreground is null)
            {
                foreground = Brushes.Black;
                if (captureRect is PixelRect rect &&
                    _screenBrightnessHelper.TryGetAverageRelativeLuminance(rect, out var luminance))
                {
                    foreground = ScreenBrightnessHelper.GetRecommendedForeground(luminance) == Colors.White
                        ? Brushes.White
                        : Brushes.Black;
                }
            }

            // 图标是 Path（几何图标）时用 Fill，图片则保持原样
            if (icon is Avalonia.Controls.Shapes.Path pathIcon)
            {
                pathIcon.Fill = foreground;
            }

            nameText.Foreground = foreground;

            // 自定义结果窗口背景色
            var background = ParseBrush(appearance.ResultBackground);
            if (background is not null)
            {
                showerWindow.Background = background;
            }
        }

        showerWindow.Show();
        try
        {
            await Task.Delay((int)(duration * 1000), token);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            // 隐藏而非关闭，保留 MaterialHost 的 GPU 管线供下次点名复用。
            showerWindow.Hide();
        }

        _logger.LogInformation("Call window hidden for text: {Text}", text);
    }

    internal void ShowCallWindow(string text, float duration, CancellationToken token) => _ = ShowCallWindowAsync(text, duration, token);

    internal void ShowHoverWindow()
    {
        HoverWindow ??= CreateHoverWindow();
        HoverWindow.Show();
        _logger.LogInformation("Hover window shown: {Theme}", HoverWindow.GetType().Name);
    }

    internal void HideHoverWindow()
    {
        HoverWindow?.Hide();
        _logger.LogInformation("Hover window hidden.");
    }

    internal void CloseHoverWindow()
    {
        HoverWindow?.Close();
        HoverWindow = null;
        _logger.LogInformation("Hover window closed.");
    }

    private Window CreateHoverWindow() => _liquidGlassRuntime.CanUseHoverTheme()
        ? new HoverLiquid()
        : new HoverFluent();

    private Window CreateShowerWindow() => _liquidGlassRuntime.CanUseShowerTheme()
        ? new LiquidShower()
        : new FluentShower();

    /// <summary>
    /// 获取可复用的展示窗口，主题切换时才重建，避免反复创建/销毁
    /// 液态玻璃窗口导致 GPU 资源泄漏。
    /// </summary>
    private Window GetOrCreateShowerWindow()
    {
        bool useLiquid = _liquidGlassRuntime.CanUseShowerTheme();
        bool currentIsLiquid = ShowerWindow is LiquidShower;

        if (ShowerWindow is not null && currentIsLiquid == useLiquid)
        {
            return ShowerWindow;
        }

        // 主题变化或首次创建：关闭旧窗口并重建
        ShowerWindow?.Close();
        ShowerWindow = null;

        var window = CreateShowerWindow();
        window.WindowStartupLocation = WindowStartupLocation.Manual;
        window.SizeToContent = SizeToContent.Manual;
        ShowerWindow = window;
        _logger.LogInformation("展示窗口已创建：{Theme}", window.GetType().Name);
        return window;
    }

    private void OnHoverSettingChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(HoverSetting.HoverTheme) || _isRecreatingHover || HoverWindow is null)
        {
            return;
        }

        _isRecreatingHover = true;
        try
        {
            bool wasVisible = HoverWindow.IsVisible;
            CloseHoverWindow();
            if (wasVisible)
            {
                ShowHoverWindow();
            }
        }
        finally
        {
            _isRecreatingHover = false;
        }
    }

    /// <summary>结果窗口名字前的图标（洗牌/随机）。</summary>
    private const string CallGlyphGeometry =
        "M10.59 9.17L5.41 4 4 5.41l5.17 5.17 1.42-1.41zM14.5 4l2.04 2.04L4 18.59 5.41 20 17.96 7.46 20 9.5V4h-5.5zm.33 9.41l-1.41 1.41 3.13 3.13L14.5 20H20v-5.5l-2.04 2.04-3.13-3.13z";

    /// <summary>根据外观设置创建结果窗口名字前的图标（内置/本地图片优先，否则用几何图标）。</summary>
    private static Control CreateResultIcon(AppearanceSetting appearance)
    {
        var size = appearance.ResultFontSize;

        var imageSource = BuiltinImages.Load(appearance.ResultImagePath);
        if (imageSource is not null)
        {
            return new Image
            {
                Source = imageSource,
                Width = size,
                Height = size,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };
        }

        return new Avalonia.Controls.Shapes.Path
        {
            Data = Geometry.Parse(CallGlyphGeometry),
            Width = size,
            Height = size,
            Stretch = Stretch.Uniform,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
    }

    private static IBrush? ParseBrush(string? hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
        {
            return null;
        }

        return Color.TryParse(hex, out var color) ? new SolidColorBrush(color) : null;
    }
}
