import numpy as np
import matplotlib.pyplot as plt

# 设置参数
wavelength = 500e-9  # 波长 (米)
thickness = np.linspace(0, 1e-6, 512)  # 薄膜厚度 (米)
n1 = 1.0  # 上方介质的折射率 (空气)
n2 = 1.5  # 薄膜的折射率 (例如：塑料)

# 计算干涉光程差
path_difference = 2 * n2 * thickness

# 计算反射强度 (简化模型)
intensity = (1 + np.cos(2 * np.pi * path_difference / wavelength)) / 2

# 创建 512x64 的颜色纹理
texture = np.zeros((64, 512, 3))  # 初始化为黑色 (RGB)
for i in range(64):
    texture[i, :, 0] = intensity  # 红色通道
    texture[i, :, 1] = intensity  # 绿色通道
    texture[i, :, 2] = intensity  # 蓝色通道

# 保存为图像
plt.imsave('thin_film_texture.png', texture)

# 可视化生成的纹理
plt.imshow(texture, extent=[0, 1, 0, 1], aspect='auto')
plt.title('Thin Film Interference Texture')
plt.xlabel('Film Thickness (normalized)')
plt.ylabel('Height (normalized)')
plt.show()