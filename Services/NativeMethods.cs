using System;
using System.Runtime.InteropServices;

namespace Classcaller.Services;

/// <summary>
/// 置顶增强所需的 Win32 互操作声明。
/// 仅使用经典 user32 API，无额外运行时依赖。
/// </summary>
internal static class NativeMethods
{
    // ---- SetWindowPos 插入顺序句柄 ----
    /// <summary>置顶带顶部（将窗口置于所有置顶窗口的最上方）。</summary>
    public static readonly IntPtr HWND_TOPMOST = new(-1);

    /// <summary>置顶带顶部（同 HWND_TOPMOST 效果，置于所属带最上）。</summary>
    public static readonly IntPtr HWND_TOP = new(0);

    // ---- SetWindowPos 标志 ----
    public const uint SWP_NOSIZE = 0x0001;
    public const uint SWP_NOMOVE = 0x0002;
    public const uint SWP_NOZORDER = 0x0004;
    public const uint SWP_NOACTIVATE = 0x0010;
    public const uint SWP_SHOWWINDOW = 0x0040;
    public const uint SWP_NOOWNERZORDER = 0x0200;
    public const uint SWP_ASYNCWINDOWPOS = 0x4000;

    // ---- GetWindowLong 索引 ----
    public const int GWL_EXSTYLE = -20;

    // ---- 扩展窗口样式 ----
    public const long WS_EX_TOPMOST = 0x00000008L;
    public const long WS_EX_TOOLWINDOW = 0x00000080L;
    public const long WS_EX_NOACTIVATE = 0x08000000L;
    public const long WS_EX_LAYERED = 0x00080000L;
    public const long WS_EX_TRANSPARENT = 0x00000020L;

    // ---- 窗口事件 ----
    public const uint EVENT_SYSTEM_FOREGROUND = 0x0003;
    public const uint EVENT_SYSTEM_MINIMIZESTART = 0x0016;
    public const uint WINEVENT_OUTOFCONTEXT = 0x0000;
    public const uint WINEVENT_SKIPOWNPROCESS = 0x0002;

    // ---- GetWindow 命令 ----
    public const uint GW_HWNDNEXT = 2;
    public const uint GW_HWNDPREV = 3;

    // ---- MonitorFromWindow ----
    public const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

    // ---- ShowWindow ----
    public const int SW_SHOWNOACTIVATE = 4;

    /// <summary>WinEvent 回调委托。</summary>
    public delegate void WinEventDelegate(
        IntPtr hWinEventHook,
        uint eventType,
        IntPtr hwnd,
        int idObject,
        int idChild,
        uint dwEventThread,
        uint dwmsEventTime);

    /// <summary>窗口矩形。</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public bool Equals(RECT other) =>
            Left == other.Left && Top == other.Top && Right == other.Right && Bottom == other.Bottom;
    }

    /// <summary>显示器信息。</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int x,
        int y,
        int cx,
        int cy,
        uint uFlags);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongW", SetLastError = true)]
    private static extern int GetWindowLong32(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
    private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongW", SetLastError = true)]
    private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    public static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

    // ---- DWM（桌面窗口管理器）----
    /// <summary>DWM 属性：窗口是否被系统隐藏（cloaked）。UI Automation 判断
    /// IsOffscreen / 隐藏状态的底层数据源即为此属性（UWP 挂起窗口、虚拟桌面
    /// 切换后的窗口、任务视图中的窗口等会返回非 0 值）。</summary>
    public const int DWMWA_CLOAKED = 14;

    [DllImport("dwmapi.dll")]
    public static extern int DwmGetWindowAttribute(
        IntPtr hwnd,
        int dwAttribute,
        out int pvAttribute,
        int cbAttribute);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr SetWinEventHook(
        uint eventMin,
        uint eventMax,
        IntPtr hmodWinEventProc,
        WinEventDelegate lpfnWinEventProc,
        uint idProcess,
        uint idThread,
        uint dwFlags);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UnhookWinEvent(IntPtr hWinEventHook);

    /// <summary>跨 32/64 位安全的 GetWindowLongPtr。</summary>
    public static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
    {
        return IntPtr.Size == 8
            ? GetWindowLongPtr64(hWnd, nIndex)
            : new IntPtr(GetWindowLong32(hWnd, nIndex));
    }

    /// <summary>跨 32/64 位安全的 SetWindowLongPtr。</summary>
    public static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
    {
        return IntPtr.Size == 8
            ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong)
            : new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
    }

    /// <summary>获取窗口扩展样式。</summary>
    public static long GetWindowExStyle(IntPtr hWnd)
    {
        var result = GetWindowLongPtr(hWnd, GWL_EXSTYLE);
        return result.ToInt64();
    }

    /// <summary>设置窗口扩展样式，返回旧值。</summary>
    public static long SetWindowExStyle(IntPtr hWnd, long newStyle)
    {
        var result = SetWindowLongPtr(hWnd, GWL_EXSTYLE, new IntPtr(newStyle));
        return result.ToInt64();
    }
}
