import numpy as np
import cv2
import os

# 创建原始的2D LUT


def create_3d_lut():
    lut_3d = np.zeros((32, 32, 32, 3), dtype=np.uint8)

    for b in range(32):
        for g in range(32):
            for r in range(32):
                lut_3d[r, g, b] = [r * 8, g * 8, b * 8]

    return lut_3d


def convert_3d_lut_to_2d_horizontal(lut_3d):
    lut_2d = np.zeros((32, 32 * 32, 3), dtype=np.uint8)

    for i in range(32):
        for j in range(32):
            lut_2d[j, 32 * i : 32 * (i + 1)] = lut_3d[i, j, :]

    return lut_2d


lut_3d = create_3d_lut()
lut_2d_horizontal = convert_3d_lut_to_2d_horizontal(lut_3d)

lut_2d_horizontal = np.flipud(lut_2d_horizontal)


current_dir = os.path.dirname(os.path.abspath(__file__))

file_path = os.path.join(current_dir, "original_lut.png")
cv2.imwrite(file_path, lut_2d_horizontal)

print(file_path)
