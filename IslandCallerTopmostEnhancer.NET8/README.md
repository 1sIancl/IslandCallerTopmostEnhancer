# IslandCaller 置顶增强（稳定版 / .NET 8）

**IslandCaller.TopmostEnhancer.NET8** · 面向 **ClassIsland 2.1.0.1 稳定版** 的
IslandCaller 悬浮窗最高级置顶增强插件。

## 适配范围

| 项目 | 版本 |
|---|---|
| ClassIsland | **2.1.0.1**（2.1 稳定线，.NET 8 + Avalonia 11.3.17） |
| IslandCaller | **2.0.1.3**（net8 老架构）、**2.1.0.0**（net10 新架构） |
| 目标框架 | net8.0（宿主 .NET 8 运行时） |
| 插件 API | apiVersion 2.1.0.1 |

> 本分支与仓库根目录的 [net10 版](../README.md)（面向 ClassIsland 2.1.1.x 2.2 技术预览线）功能完全一致，仅适配不同宿主技术栈。按 ClassIsland 版本二选一安装，勿同时启用。

## 功能（七机制协同）

① 高频 Z 序重推（250ms `SetWindowPos(HWND_TOPMOST)`，可调 50~2000ms）
② 前台事件钩子（`SetWinEventHook` 监听 `EVENT_SYSTEM_FOREGROUND`）
③ 扩展样式强化（`WS_EX_TOPMOST` / `WS_EX_TOOLWINDOW` / `WS_EX_NOACTIVATE`）
④ Z 序校验自动恢复（`GetWindow(GW_HWNDPREV)` 失效即恢复）
⑤ 全屏抢占检测（前台窗口矩形 vs 显示器边界）
⑥ 窗口自动发现（程序集名 `IslandCaller*` + 类型名 + 标题关键词，跨插件隔离零依赖）
⑦ UIA 增强检测（DWM `DWMWA_CLOAKED` 语义，识别被隐藏的 UWP/现代化窗口）

窗口识别兼容 IslandCaller **2.0.1.3**（HoverFluent / PersonalCall）与
**2.1.0.0**（HoverFluent / HoverLiquid / FluentShower / LiquidShower / PersonalCall）。

## 安装

1. 从 Releases 下载 `IslandCaller.TopmostEnhancer.NET8.cipx`；
2. 放入 ClassIsland 2.1.0.1 的 `Plugins` 目录；
3. 在【应用设置 → 插件】中启用"**IslandCaller 置顶增强（稳定版）**"。

> 注意：**请勿**同时安装 net10 版（IslandCallerTopmostEnhancer）与 net8 版——
> 二者功能相同，重复安装无意义。按你的 ClassIsland 版本二选一：
> - ClassIsland 2.1.0.1（稳定版）→ 本插件
> - ClassIsland 2.1.1.0 / 2.1.1.1（2.2 预览）→ net10 版

## 设置

【应用设置 → 插件 → IslandCaller 置顶增强（稳定版）】：
总开关 / Z 序重推周期 / 前台事件钩子 / 强制置顶样式 / 从 Alt+Tab 隐藏 /
不抢焦点 / UIA 增强检测 / 额外匹配标题关键词。配置自动保存于插件配置目录
`Settings.json`。

## 构建

```bash
# 需要 .NET 8 SDK。ClassIslandRuntimeDir 指向 ClassIsland 2.1.0.1 发布包解压目录
dotnet build IslandCallerTopmostEnhancer.NET8.csproj -c Release --nologo
```

## 许可证

GPL-3.0
