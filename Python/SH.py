import numpy as np
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d import Axes3D
from scipy.special import sph_harm

# 定义球坐标
phi = np.linspace(0, np.pi, 100)  # 方位角
theta = np.linspace(0, 2 * np.pi, 100)  # 极角
phi, theta = np.meshgrid(phi, theta)

# 计算球谐函数
m, l = 0, 3  # m 和 l 是球谐函数的参数
Yml = sph_harm(m, l, theta, phi).real  # 计算实部

# 将球坐标转换为笛卡尔坐标
r = abs(Yml)  # 半径
x = r * np.sin(phi) * np.cos(theta)
y = r * np.sin(phi) * np.sin(theta)
z = r * np.cos(phi)

# 创建 3D 图形
fig = plt.figure(figsize=(8, 8))
ax = fig.add_subplot(111, projection="3d")
ax.plot_surface(x, y, z, rstride=1, cstride=1, color="c", alpha=0.6, linewidth=0)

# 显示图形
plt.show()
