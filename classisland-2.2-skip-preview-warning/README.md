# ClassIsland 2.2 跳过开屏预览警告弹窗

给 ClassIsland **2.2（Misha Developer Preview）** 打一个补丁，让它在启动时不显示
「欢迎使用 2.2-Misha Developer Preview」的技术性警告弹窗，直接进入主界面。

## 为什么是「补丁」而不是「插件」

ClassIsland 的这个警告弹窗在启动流程的**最早阶段**就弹出了，插件根本来不及加载：

| 阶段 | 代码位置 | 说明 |
|---|---|---|
| ① 警告弹窗 | `ClassIsland/App.axaml.cs` 的 `App.Init()` 早期（约 580–599 行） | `await FATaskDialog.ShowAsync()` **模态阻塞**，等用户点「确定」 |
| ② 插件加载 | 同一文件约 707 行之后 | 要等 Host 构建完成后才调用插件的 `Initialize` |

所以弹窗显示时插件还没加载、插件加载时弹窗早被点掉了——**任何 `.cipx` 插件都无法拦截它**。
只能通过修改 ClassIsland 本体源码来跳过，本目录提供的就是这个最小改动。

## 补丁内容

只改一个文件：`ClassIsland/App.axaml.cs`，两处改动：

1. 新增开关字段（默认 `false` = 跳过）：
   ```csharp
   internal static readonly bool ShowDeveloperPreviewWarning = false;
   ```
2. 把弹窗调用用 `if (ShowDeveloperPreviewWarning)` 包裹（保留 `#if RELEASE` 原条件）。

改动用 `static readonly`（非 `const`）避免 CS0162 死代码警告；其他启动检测
（临时目录 / 桌面 / 目录权限 / 恢复模式等）**一律未动**。

## 应用补丁

在 ClassIsland 源码根目录（`develop/v2/misha-alpha` 分支）执行：

```bash
git apply classisland-2.2-skip-preview-warning.patch
```

或直接按上面的说明手动改 `ClassIsland/App.axaml.cs`。

## 编译

> 前置：`.NET 10 SDK`、完整源码（含 `vendors/EdgeTtsSharp` 子模块）。

```bash
dotnet publish ClassIsland.Desktop/ClassIsland.Desktop.csproj \
  -c Release \
  -p:PublishBuilding=true \
  -p:PublishPlatform=windows \
  -p:RuntimeIdentifier=win-x64 \
  -p:ClassIsland_PlatformTarget=x64 \
  -p:SelfContained=true \
  -p:ClassIsland_SelfContained=true \
  -o out
```

**注意**：`PublishBuilding=true` 时官方需要 `ClassIsland/secrets.g.cs`（GPT-SoVits 签名密钥），
本地无密钥时需先在该路径放一个占位文件：

```csharp
namespace ClassIsland.Services.SpeechService;

public static partial class GptSovitsSecrets
{
    public const string PrivateKey = "";
    public const string PrivateKeyPassPhrase = "";
    public const bool IsSecretsFilled = false;
}
```

`IsSecretsFilled=false` 会自动停用加密签名，不影响其他功能。

## 恢复显示警告

把 `ShowDeveloperPreviewWarning` 改成 `true` 重新编译即可。

## 说明

- 本补丁仅针对 2.2 技术预览版的临时弹窗（官方代码自带 `// TODO: 退出 DP 后记得删`，
  正式版发布后该弹窗会被官方删除，届时本补丁自动失效、无需再应用）。
- 补丁基于 ClassIsland `develop/v2/misha-alpha` 分支（`2.1.1.1-20-g50414605`）。
