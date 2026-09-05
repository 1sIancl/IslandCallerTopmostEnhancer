# -*- coding: utf-8 -*-
"""生成 Classcaller 内置图片（悬浮窗/结果窗口可选的预设图标）。

白色实心图标 + 透明背景，128x128，用于嵌入插件资源。
"""
import math
from PIL import Image, ImageDraw

SIZE = 128
WHITE = (255, 255, 255, 255)


def new_canvas():
    return Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))


def icon_dice():
    """骰子：圆角方形 + 5 个点。"""
    img = new_canvas()
    d = ImageDraw.Draw(img)
    d.rounded_rectangle([14, 14, 114, 114], radius=26, outline=WHITE, width=10)
    pts = [(36, 36), (92, 36), (64, 64), (36, 92), (92, 92)]
    for cx, cy in pts:
        d.ellipse([cx - 9, cy - 9, cx + 9, cy + 9], fill=WHITE)
    return img


def icon_list():
    """名单：圆角矩形 + 3 条横线。"""
    img = new_canvas()
    d = ImageDraw.Draw(img)
    d.rounded_rectangle([20, 14, 108, 114], radius=22, outline=WHITE, width=9)
    for i in range(3):
        y = 40 + i * 26
        d.rounded_rectangle([36, y, 92, y + 12], radius=6, fill=WHITE)
    return img


def icon_star():
    """五角星。"""
    img = new_canvas()
    d = ImageDraw.Draw(img)
    cx, cy, R, r = 64, 64, 52, 21
    pts = []
    for i in range(10):
        ang = -math.pi / 2 + i * math.pi / 5
        rad = R if i % 2 == 0 else r
        pts.append((cx + rad * math.cos(ang), cy + rad * math.sin(ang)))
    d.polygon(pts, fill=WHITE)
    return img


def icon_check():
    """对勾。"""
    img = new_canvas()
    d = ImageDraw.Draw(img)
    d.line([(24, 66), (52, 94), (104, 34)], fill=WHITE, width=20, joint="curve")
    return img


def icon_trophy():
    """奖杯（简化）。"""
    img = new_canvas()
    d = ImageDraw.Draw(img)
    # 杯身
    d.polygon([(24, 40), (104, 40), (94, 84), (34, 84)], fill=WHITE)
    # 杯柄
    d.rectangle([44, 84, 84, 96], fill=WHITE)
    # 底座
    d.rounded_rectangle([26, 96, 102, 114], radius=8, fill=WHITE)
    # 杯口两侧把手
    d.arc([8, 40, 40, 76], start=-90, end=90, fill=WHITE, width=10)
    d.arc([88, 40, 120, 76], start=90, end=270, fill=WHITE, width=10)
    return img


def icon_shuffle():
    """随机/洗牌（两个交叉箭头）。"""
    img = new_canvas()
    d = ImageDraw.Draw(img)
    w = 18
    # 左上箭头（右下方向）
    d.line([(30, 24), (104, 98)], fill=WHITE, width=w)
    d.polygon([(78, 22), (108, 22), (108, 52)], fill=WHITE)
    # 左下箭头（右上方向）
    d.line([(30, 104), (104, 30)], fill=WHITE, width=w)
    d.polygon([(78, 106), (108, 106), (108, 76)], fill=WHITE)
    return img


def main():
    icons = {
        "dice": icon_dice,
        "list": icon_list,
        "star": icon_star,
        "check": icon_check,
        "trophy": icon_trophy,
        "shuffle": icon_shuffle,
    }
    import os
    out = os.path.join(os.path.dirname(os.path.abspath(__file__)), "Assets")
    os.makedirs(out, exist_ok=True)
    for name, fn in icons.items():
        fn().save(os.path.join(out, f"{name}.png"))
        print(f"生成 {name}.png")


if __name__ == "__main__":
    main()
