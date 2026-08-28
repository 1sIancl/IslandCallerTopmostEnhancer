import hashlib, os, shutil, sys, zipfile

root = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
version = sys.argv[1] if len(sys.argv) > 1 else "1.0.0.0"
build = os.path.join(root, "bin", "Release", "net10.0")
out_dir = os.path.join(root, "artifacts", "release", version)
plugin_dir = os.path.join(out_dir, "plugin")
os.makedirs(plugin_dir, exist_ok=True)

# 1) 复制构建产物（排除 pdb，保留 deps.json）
for name in os.listdir(build):
    if name.endswith(".pdb"):  # deps.json 必须保留：ClassIsland 插件加载器（AssemblyDependencyResolver）依赖它
        continue
    shutil.copy2(os.path.join(build, name), os.path.join(plugin_dir, name))

# 2) 更新 manifest 版本
manifest_path = os.path.join(plugin_dir, "manifest.yml")
with open(manifest_path, "r", encoding="utf-8") as f:
    manifest = f.read()
manifest = __import__("re").sub(r"(?m)^version: .*$", f"version: {version}", manifest)
with open(manifest_path, "w", encoding="utf-8", newline="\n") as f:
    f.write(manifest)

# 3) 打包 .cipx（zip 根目录直接放文件）
cipx = os.path.join(out_dir, "IslandCaller.TopmostEnhancer.cipx")
with zipfile.ZipFile(cipx, "w", zipfile.ZIP_DEFLATED) as zf:
    for name in os.listdir(plugin_dir):
        zf.write(os.path.join(plugin_dir, name), arcname=name)

md5 = hashlib.md5(open(cipx, "rb").read()).hexdigest()
print(f"cipx: {cipx}")
print(f"size: {os.path.getsize(cipx)} bytes")
print(f"md5 : {md5}")
print("contents:", sorted(os.listdir(plugin_dir)))
