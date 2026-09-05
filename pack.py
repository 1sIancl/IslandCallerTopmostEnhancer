import zipfile
import os
import hashlib
import sys

# 打包 Classcaller 插件为 .cipx（本质是 zip，排除 .pdb）
def main():
    src = os.path.join(
        os.path.dirname(os.path.abspath(__file__)),
        "bin", "Release", "net10.0-windows10.0.19041.0",
    )
    out = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "Classcaller.cipx")

    if not os.path.isdir(src):
        print(f"[错误] 找不到编译产物目录: {src}")
        sys.exit(1)

    count = 0
    with zipfile.ZipFile(out, "w", zipfile.ZIP_DEFLATED) as z:
        for f in sorted(os.listdir(src)):
            if f.endswith(".pdb"):
                continue
            full = os.path.join(src, f)
            if os.path.isfile(full):
                z.write(full, f)
                count += 1

    size = os.path.getsize(out)
    md5 = hashlib.md5(open(out, "rb").read()).hexdigest()
    print(f"打包完成: {out}")
    print(f"文件数: {count}, 大小: {size} bytes, MD5: {md5}")


if __name__ == "__main__":
    main()
