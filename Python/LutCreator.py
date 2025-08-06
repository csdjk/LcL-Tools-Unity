import numpy as np
import cv2
import os

# 新增常量，用于切换生成 32 或 16 的 LUT
LUT_SIZE = 32  # 可设置为 16 或 32

# 新增常量，用于控制是否翻转 Y 轴
FLIP_Y_AXIS = False  # 设置为 True 时翻转 Y 轴

def make_gray_source():
    """生成灰度信息，步长根据 LUT_SIZE 均匀分布"""
    step = 255 // (LUT_SIZE - 1)  # 动态计算步长，使灰度值均匀分布在 0-255
    return [i * step for i in range(LUT_SIZE)]

def make_lattice(source):
    """生成大小为 LUT_SIZE 的格子"""
    tex2d = np.ndarray((LUT_SIZE, LUT_SIZE, 3), dtype=np.uint8)

    for _ in range(LUT_SIZE):
        tex2d[0:LUT_SIZE, _, 2] = source[_]
    for _ in range(LUT_SIZE):
        tex2d[_, 0:LUT_SIZE, 1] = source[LUT_SIZE - 1 - _]
    return tex2d

def genlut():
    """生成 LUT"""
    source = make_gray_source()
    lattice = make_lattice(source)

    o = list()
    for _ in range(LUT_SIZE):
        _lattice = np.copy(lattice)
        _lattice[0:LUT_SIZE, 0:LUT_SIZE, 0] = source[_]
        o.append(_lattice)

    # 将生成的 LUT 拼接为一个大图像
    result = np.hstack(o)

    # 根据 FLIP_Y_AXIS 常量决定是否翻转 Y 轴
    if FLIP_Y_AXIS:
        result = np.flip(result, axis=0)

    return result

tex2d = genlut()

current_dir = os.path.dirname(os.path.abspath(__file__))
file_path = os.path.join(current_dir, f"NaturalLut{LUT_SIZE}{'_Flipped' if FLIP_Y_AXIS else ''}.png")

cv2.imwrite(file_path, tex2d)