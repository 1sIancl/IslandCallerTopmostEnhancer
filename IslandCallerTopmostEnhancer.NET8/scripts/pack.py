import hashlib, os, shutil, sys, zipfile, re
root = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
version = sys.argv[1] if len(sys.argv) > 1 else "1.0.0.0"
build = os.path.join(root, "bin", "Release", "net8.0-windows")
out_dir = os.path.join(root, "artifacts", "release", version)
plugin_dir = os.path.join(out_dir, "plugin")
os.makedirs(plugin_dir, exist_ok=True)
for name in os.listdir(build):
    if name.endswith(".pdb"):  # deps.json 必须保留：ClassIsland 2.1.0.1 插件加载器依赖它解析入口程序集
        continue
    shutil.copy2(os.path.join(build, name), os.path.join(plugin_dir, name))
manifest_path = os.path.join(plugin_dir, "manifest.yml")
with open(manifest_path, "r", encoding="utf-8") as f:
    manifest = f.read()
manifest = re.sub(r"(?m)^version: .*$", f"version: {version}", manifest)
with open(manifest_path, "w", encoding="utf-8", newline="\n") as f:
    f.write(manifest)
cipx = os.path.join(out_dir, "IslandCaller.TopmostEnhancer.NET8.cipx")
with zipfile.ZipFile(cipx, "w", zipfile.ZIP_DEFLATED) as zf:
    for name in os.listdir(plugin_dir):
        zf.write(os.path.join(plugin_dir, name), arcname=name)
md5 = hashlib.md5(open(cipx, "rb").read()).hexdigest()
print(f"cipx: {cipx}")
print(f"size: {os.path.getsize(cipx)} bytes")
print(f"md5 : {md5}")
print("contents:", sorted(os.listdir(plugin_dir)))
