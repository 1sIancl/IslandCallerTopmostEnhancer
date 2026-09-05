using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ClassIslandPatcher;

/// <summary>
/// ClassIsland 2.2（Misha Developer Preview）启动预览警告弹窗跳过补丁器。
///
/// 原理：警告弹窗在 App.&lt;Init&gt;d__N::MoveNext() 里，编译后是一段
///   newobj FATaskDialog → 若干 set_* → callvirt ShowAsync → await(GetResult→pop)
/// 本工具找到这段代码，把开头的 newobj 替换成无条件跳转（br），直接跳过整个弹窗块。
/// 只改一条 IL 指令，不动栈平衡与 async 状态机，安全可逆（自动备份 .bak）。
/// </summary>
internal static class Program
{
    private const string Marker = "欢迎使用 2.2-Misha Developer Preview";

    private static int Main(string[] args)
    {
        try { Console.OutputEncoding = System.Text.Encoding.UTF8; } catch { }

        string dllPath = args.Length > 0 ? args[0] : "ClassIsland.dll";

        if (!File.Exists(dllPath))
        {
            Console.Error.WriteLine($"[错误] 找不到文件：{dllPath}");
            Console.Error.WriteLine("用法：ClassIslandPatcher <ClassIsland.dll 的完整路径>");
            Console.Error.WriteLine("提示：若不确定路径，可把本程序放到 ClassIsland 安装目录，直接双击运行（不带参数）。");
            return 1;
        }

        string fullPath = Path.GetFullPath(dllPath);
        Console.WriteLine($"目标文件：{fullPath}");

        // 备份（只备份一次，保留原始官方 dll）
        string bakPath = fullPath + ".bak";
        if (!File.Exists(bakPath))
        {
            File.Copy(fullPath, bakPath, overwrite: false);
            Console.WriteLine($"已备份原始文件到：{bakPath}");
        }
        else
        {
            Console.WriteLine($"检测到已有备份：{bakPath}（跳过备份）");
        }

        AssemblyDefinition asm;
        try
        {
            var resolver = new DefaultAssemblyResolver();
            string dllDir = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(dllDir))
            {
                resolver.AddSearchDirectory(dllDir);
            }
            asm = AssemblyDefinition.ReadAssembly(fullPath,
                new ReaderParameters { ReadSymbols = false, AssemblyResolver = resolver });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[错误] 无法读取 dll（若 ClassIsland 正在运行会锁定文件，请先完全退出 ClassIsland 再试）：{ex.Message}");
            return 3;
        }

        // 1. 定位弹窗代码
        MethodDefinition targetMethod = null;
        Instruction marker = null;
        foreach (var type in asm.MainModule.GetTypes())
        {
            foreach (var m in type.Methods)
            {
                if (!m.HasBody) continue;
                foreach (var ins in m.Body.Instructions)
                {
                    if (ins.OpCode == OpCodes.Ldstr && ins.Operand is string s && s == Marker)
                    {
                        targetMethod = m;
                        marker = ins;
                        break;
                    }
                }
                if (targetMethod != null) break;
            }
            if (targetMethod != null) break;
        }

        if (targetMethod == null || marker == null)
        {
            Console.Error.WriteLine("[错误] 未找到预览警告弹窗代码。可能原因：");
            Console.Error.WriteLine("  1) 该 dll 不是 ClassIsland 2.2（或版本不匹配）；");
            Console.Error.WriteLine("  2) 已被本工具 patch 过（新指令已跳过了这段，可检查是否存在 .bak 备份）。");
            return 2;
        }

        // 2. 起点：从 marker 向前找最近的 newobj（FATaskDialog 构造）
        Instruction start = null;
        for (var it = marker; it != null; it = it.Previous)
        {
            if (it.OpCode == OpCodes.Newobj)
            {
                start = it;
                break;
            }
        }
        if (start == null || start.Operand is not MethodReference ctor || !ctor.DeclaringType.FullName.Contains("FATaskDialog"))
        {
            Console.Error.WriteLine("[错误] 未能在弹窗字符串前定位到 FATaskDialog 构造（可能已 patch 过或版本不匹配）。");
            return 4;
        }

        // 3. 终点：从 marker 向后找 ShowAsync 调用，再找其 await 完成（GetResult）后的下一条指令
        Instruction showAsync = null;
        for (var it = marker; it != null; it = it.Next)
        {
            if (it.OpCode == OpCodes.Callvirt && it.Operand is MethodReference mr && mr.Name == "ShowAsync")
            {
                showAsync = it;
                break;
            }
        }
        if (showAsync == null)
        {
            Console.Error.WriteLine("[错误] 未找到 ShowAsync 调用。");
            return 5;
        }

        Instruction getResult = null;
        for (var it = showAsync.Next; it != null; it = it.Next)
        {
            if ((it.OpCode == OpCodes.Call || it.OpCode == OpCodes.Callvirt) &&
                it.Operand is MethodReference mr && mr.Name == "GetResult")
            {
                getResult = it;
                break;
            }
        }
        if (getResult == null)
        {
            Console.Error.WriteLine("[错误] 未找到 await 完成点（GetResult）。");
            return 6;
        }

        // GetResult 后是 pop（丢弃 await 结果），终点取 pop 之后
        Instruction end = getResult.Next;
        if (end != null && end.OpCode == OpCodes.Pop)
        {
            end = end.Next;
        }
        if (end == null)
        {
            Console.Error.WriteLine("[错误] 无法确定弹窗代码的结束位置。");
            return 7;
        }

        // 4. 替换：把起点 newobj 改为 br 跳到终点
        var il = targetMethod.Body.GetILProcessor();
        var br = il.Create(OpCodes.Br, end);
        il.Replace(start, br);

        // 5. 写回（先写临时文件，释放读锁后再替换原文件）
        string tempPath = fullPath + ".patched.tmp";
        try
        {
            asm.Write(tempPath);
            asm.Dispose();
        }
        catch (Exception ex)
        {
            try { asm.Dispose(); } catch { }
            Console.Error.WriteLine($"[错误] 写入失败：{ex.Message}");
            return 8;
        }

        try
        {
            File.Copy(tempPath, fullPath, overwrite: true);
            File.Delete(tempPath);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[错误] 替换文件失败（dll 可能被 ClassIsland 进程占用，请完全退出后重试）：{ex.Message}");
            return 9;
        }

        Console.WriteLine();
        Console.WriteLine("✔ 补丁成功！预览警告弹窗已被跳过，启动 ClassIsland 2.2 时将直接进入主界面。");
        Console.WriteLine($"  方法：{targetMethod.FullName}");
        Console.WriteLine($"  原备份：{bakPath}（如需恢复，用备份覆盖原 dll 即可）");
        return 0;
    }
}
