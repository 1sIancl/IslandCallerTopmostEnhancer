<div align="center">

# IslandCaller 置顶增强

**IslandCaller.TopmostEnhancer** · 让 IslandCaller 悬浮窗永远站在屏幕最顶端

一个用于 **ClassIsland 2.1.1.x** 的插件，将 [IslandCaller](https://github.com/HickoryTrail/IslandCaller)点名器的悬浮窗与结果窗口置顶优先级提升到**用户态 Windows 的最高级别**——即使在全屏 PPT、白板、直播投屏、视频全屏或其它置顶窗口抢占下，也始终保持在最上层。

![Platform](https://img.shields.io/badge/platform-Windows%2010%202004%2B-blue)
![ClassIsland](https://img.shields.io/badge/ClassIsland-2.1.1.0%20%2F%202.1.1.1-orange)
![.NET](https://img.shields.io/badge/.NET-10-purple)
![License](https://img.shields.io/badge/License-GPL--3.0-green)

</div>

## 版本选择

本仓库同时提供两个技术栈的插件，**按你的 ClassIsland 版本二选一，勿同时装**：

| 版本 | 适用 ClassIsland | 适用 IslandCaller | 技术栈 | 目录 / 下载 |
|---|---|---|---|---|
| **net10 版**（本页） | 2.1.1.0 / 2.1.1.1（2.2 技术预览线） | 2.1.x（含 2.1.0.0） | .NET 10 + Avalonia 12 | 仓库根目录；Releases 中 `IslandCaller.TopmostEnhancer.cipx` |
| **稳定版（net8）** | **2.1.0.1（2.1 稳定线）** | **2.0.1.3** / 2.1.0.0 | .NET 8 + Avalonia 11.3.17 | [`IslandCallerTopmostEnhancer.NET8/`](IslandCallerTopmostEnhancer.NET8/)；Releases 中 `IslandCaller.TopmostEnhancer.NET8.cipx` |

两个版本功能完全一致（七机制协同最高置顶 + UIA 增强检测），仅适配不同宿主技术栈。



## 特性

-  **用户态最高置顶**：置顶带（topmost band）最顶端 + 持续占据 + 失效即时恢复；
-  **六机制协同**：高频 Z 序重推 / 前台事件钩子 / 扩展样式强化 / Z 序校验自动恢复 /
  全屏抢占检测 / 窗口自动发现，互相独立、可单独开关；
-  **不抢焦点**：全程 `WS_EX_NOACTIVATE` + `SWP_NOACTIVATE`，点名悬浮窗不会打断教师操作；
-  **Alt+Tab 隐身**：`WS_EX_TOOLWINDOW` 使悬浮窗不进入任务切换列表；
-  **零耦合**：不依赖 IslandCaller 任何程序集，跨插件程序集隔离，可独立安装/卸载；
-  **可视设置**：ClassIsland 设置页内可调总开关、重推周期与各机制开关。

## 环境要求

| 项目 | 要求 |
|---|---|
| 操作系统 | Windows 10 2004（10.0.19041）及以上，x64 |
| 主程序 | ClassIsland **2.1.1.0 / 2.1.1.1**（2.2 技术预览线） |
| 前置插件 | IslandCaller（需要其悬浮窗 / 结果窗口保持置顶时） |

## 安装

1. 从 [Releases](../../releases/latest) 下载 `IslandCaller.TopmostEnhancer.cipx`（下载后请核对 MD5）；
3. 在 ClassIsland 的【应用设置 → 插件】中启用"**IslandCaller 置顶增强**"；

## 使用与设置

插件设置位于【应用设置 → 插件 → IslandCaller 置顶增强】：

- **启用最高置顶增强**：总开关；
- **Z 序重推周期**：建议 100~500ms；
- **前台事件钩子 / 强制置顶样式 / 从 Alt+Tab 隐藏 / 不抢焦点 / UIA 增强检测**：各机制独立开关；
- **额外匹配的标题关键词**：窗口标题包含任一关键词即纳入最高置顶。

设置保存在插件配置目录的 `Settings.json`，修改即自动保存。

## 代码结构

```plaintext
IslandCallerTopmostEnhancer/
├── IslandCaller.TopmostEnhancer.csproj   // 工程文件（net10.0，直接引用宿主程序集）
├── manifest.yml                           // 插件清单
├── Plugin.cs                              // 插件入口：加载配置、注册服务、启动引擎
├── Models/
│   └── Settings.cs                        // 配置模型（自动持久化 Settings.json）
├── Services/
│   ├── NativeMethods.cs                   // Win32 P/Invoke 层（SetWindowPos / SetWindowLongPtr /
│   │                                      //   SetWinEventHook / GetWindow / GetMonitorInfo …）
│   └── TopmostEnhancerService.cs          // 核心引擎：定时重推 + 前台钩子 + 样式强化 +
│                                          //   Z 序校验自动恢复 + 全屏抢占检测 + 窗口发现
├── ViewModels/
│   └── SettingsPageViewModel.cs           // 设置页视图模型
├── Views/
│   └── SettingsPage.axaml.cs              // 设置页（纯代码构建）
├── scripts/
│   ├── Package-Release.ps1                // 打包脚本（.cipx + MD5）
│   └── pack.py                            // 备选打包脚本（无 PowerShell 依赖）
└── icon.png / README.md / LICENSE
```

### 核心调用链（TopmostEnhancerService）

```text
Start()                                  // AppStarted 后调用
├── RestartTimer()                       // DispatcherTimer（默认 250ms）
├── InstallForegroundHook()              // SetWinEventHook(EVENT_SYSTEM_FOREGROUND)
└── ScanAndApply()                       // 立即执行一次

每次 Tick（UI 线程）：
ScanAndApply()
├── 枚举 lifetime.Windows 识别 IslandCaller 窗口 → 更新 _trackedHwnds
├── IsForegroundFullscreen() ? → ApplyAllTracked()  // 全屏抢占立即重推
├── 对每个句柄：ApplyAll(hwnd)
│   ├── SetWindowPos(HWND_TOPMOST, SWP_NOACTIVATE|SWP_ASYNCWINDOWPOS)     // 机制①
│   └── SetWindowLongPtr(WS_EX_TOPMOST|WS_EX_TOOLWINDOW|WS_EX_NOACTIVATE) // 机制③
└── HasVisibleWindowAbove(hwnd) ? → ApplyAll(hwnd)  // 机制④ 失效即恢复

前台钩子回调（任意窗口抢前台）：
OnWinEvent → 节流 50ms → Dispatcher.UIThread.Post(ApplyAllTracked)  // 机制②
```

## 自动恢复机制

- **被其它置顶窗口抢占**：其它置顶窗口在之后被置顶时会压在本窗口上方 →
  ① 的高频重推与 ④ 的 Z 序校验会在 ≤250ms 内重新夺回置顶带最顶端；
- **用户切换应用 / 全屏应用抢焦点**：② 前台事件钩子立即触发整体重推；
- **窗口被新创建 / 重新打开**：⑥ 窗口发现每次扫描都会重新纳入并立即置顶；
- **全屏遮挡**：⑤ 全屏检测在扫描周期内额外触发一次整体重推。

> **说明**：用户态普通窗口无法覆盖"独占全屏"（exclusive fullscreen，仅见于游戏）。
> 课堂场景的 PPT / 白板 / 直播 / 视频全屏均为无边框窗口，置顶带可稳压在上。

## 开发与构建

技术栈：.NET 10 / Avalonia 12 / ClassIsland 2.1.1.1（PluginSdk 2.1.1.1 同源 API）

```bash
# 构建（Release）。ClassIslandRuntimeDir 指向 ClassIsland 2.1.1.1 发布包解压目录
# （默认 ../ci2111/app-2.1.1.1-0，可用 -p:ClassIslandRuntimeDir=... 覆盖）
dotnet build IslandCaller.TopmostEnhancer.csproj -c Release --nologo

# 本地打包（生成 .cipx，PowerShell）
powershell -File scripts\Package-Release.ps1 -Version 1.0.0.0

# 本地打包（备选，Python）
python scripts\pack.py 1.0.0.0
```

## 平台适配说明

- **Windows（主要目标）**：全部机制可用，达到用户态最高置顶优先级；
- **Android / iOS / Linux**（ClassIsland 2.2 跨平台预览）：`Start()` 检测到非
  Windows 平台时直接返回并记录日志，插件空载运行，不影响宿主；
- **独占全屏应用（游戏）**：系统限制下用户态窗口无法覆盖，属正常行为。

## 常见问题

- **与 IslandCaller "置顶"冲突吗？**
  不冲突。本插件在 IslandCaller 自身置顶（3000ms 循环）的基础上，以更短周期
  （250ms）+ 事件钩子 + Z 序校验进一步强化，二者互不干扰。
- **卸载后会有残留吗？**
  不会。停用插件即停止全部机制，不再写入任何窗口样式；IslandCaller 原有置顶功能不受影响。
- **为什么偶尔看到悬浮窗被全屏游戏盖住？**
  独占全屏（exclusive fullscreen）由显卡驱动接管，用户态窗口无法覆盖，这是系统限制；
  改为无边框全屏（窗口化全屏）即可被置顶带压制。
- **置顶后抢焦点怎么办？**
  默认已启用 `WS_EX_NOACTIVATE` 与 `SWP_NOACTIVATE`，置顶全程不会夺取输入焦点；
  如需点击悬浮窗交互，点击时会正常激活（IslandCaller 自身行为）。

## 致谢

- [ClassIsland](https://github.com/ClassIsland/ClassIsland) —— 插件宿主框架（LGPL-3.0）
- [IslandCaller](https://github.com/HickoryTrail/IslandCaller) —— 被增强的目标点名插件（GPL-3.0）

## 许可证

[GPL-3.0](LICENSE)
