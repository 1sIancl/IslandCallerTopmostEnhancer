# ClassIsland 2.2 跳过开屏预览警告弹窗

让 ClassIsland **2.2（Misha Developer Preview）** 启动时不显示
「欢迎使用 2.2-Misha Developer Preview」技术性警告弹窗，直接进入主界面。

提供两种方式，**推荐方式一（patcher，免编译，双击即用）**。

---

## 方式一：patcher（推荐，免编译）

`patcher/` 里是一个独立的补丁程序，直接修改你**已安装**的 ClassIsland 2.2 的
`ClassIsland.dll`，不用重新编译。

### 用法

1. 下载 `ClassIslandPatcher.exe`（见 GitHub Release 资产，或自行编译）。
2. 把它放到 ClassIsland 安装目录（与 `ClassIsland.dll` 同级）。
3. **完全退出 ClassIsland**（托盘图标右键退出），否则 dll 被占用无法写入。
4. 双击运行 `ClassIslandPatcher.exe`，看到「✔ 补丁成功」即可。

> 也可带路径参数运行：`ClassIslandPatcher.exe "D:\ClassIsland\ClassIsland.dll"`

### 恢复

patcher 会自动备份原文件为 `ClassIsland.dll.bak`。用备份覆盖回去即可还原。

### 自行编译 patcher

```bash
dotnet publish patcher/ClassIslandPatcher.csproj -c Release -r win-x64 --self-contained true \
  -p:PublishSingleFile=true -o dist
```

---

## 方式二：源码补丁 + 自行编译

给 ClassIsland 源码打补丁后重新编译（适合想自己掌控构建的人）。

### 为什么是「补丁」而不是「插件」

这个警告弹窗在启动流程的**最早阶段**就弹出了，插件根本来不及加载：

| 阶段 | 代码位置 | 说明 |
|---|---|---|
| ① 警告弹窗 | `ClassIsland/App.axaml.cs` 的 `App.Init()` 早期 | `await FATaskDialog.ShowAsync()` **模态阻塞** |
| ② 插件加载 | 同一文件约 707 行之后 | 要等 Host 构建完成后才调用插件 `Initialize` |

所以弹窗显示时插件还没加载、插件加载时弹窗早被点掉了——**任何 `.cipx` 插件都无法拦截它**。

### 应用补丁

在 ClassIsland 源码根目录（`develop/v2/misha-alpha` 分支）执行：

```bash
git apply classisland-2.2-skip-preview-warning.patch
```

### 编译

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

---

## 原理

补丁只做一件事：把警告弹窗那段代码开头的 `newobj FATaskDialog` 替换成无条件跳转 `br`，
直接跳过整个弹窗块（含 `ShowAsync` 与 await）。只改一条 IL 指令，不动栈平衡与 async
状态机，安全可逆。其他启动检测（临时目录 / 桌面 / 目录权限 / 恢复模式）一律不动。

## 说明

- 本补丁仅针对 2.2 技术预览版的临时弹窗（官方代码自带 `// TODO: 退出 DP 后记得删`，
  正式版发布后该弹窗会被官方删除，届时本补丁自动失效）。
- 补丁基于 ClassIsland `develop/v2/misha-alpha` 分支（`2.1.1.1-20-g50414605`）。
