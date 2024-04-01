import numpy as np
import cv2
import os


def make_gray_source_32():
    """生成步长为32的灰度信息"""

    o = [0]
    step = 0
    for _ in range(1, 32):
        step += 9 if _ % 4 == 0 else 8
        o.append(step)
    return o


def make_lattice(source):
    """生成大小为32的格子"""

    tex2d = np.ndarray((32, 32, 3), dtype=np.uint8)

    for _ in range(32):
        tex2d[0:32, _, 2] = source[_]
    for _ in range(32):
        tex2d[_, 0:32, 1] = source[31 - _]
    return tex2d


def genlut32():

    source = make_gray_source_32()
    lattice = make_lattice(source)

    o = list()
    for _ in range(32):
        _lattice = np.copy(lattice)
        _lattice[0:32, 0:32, 0] = source[_]
        o.append(_lattice)
    return np.hstack(o)


tex2d = genlut32()


current_dir = os.path.dirname(os.path.abspath(__file__))
file_path = os.path.join(current_dir, "NaturalLut32.png")

cv2.imwrite(file_path, tex2d)
