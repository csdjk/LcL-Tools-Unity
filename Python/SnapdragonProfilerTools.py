"""
这个脚本是一个使用 PyQt6 和 pyqtgraph 库的绘图工具，用于将 Snapdragon Profiler 的截帧数据 CSV 文件可视化成柱状图。
主要功能包括：
- 加载 CSV 文件并解析数据
- 使用 pyqtgraph 绘制柱状图
- 在图表上显示提示信息和高亮效果
- 提供用户界面，允许用户选择和加载 CSV 文件

主要组件：
- CustomBarGraphItem: 自定义的柱状图项，支持鼠标悬停提示和高亮效果
- DrawCallPlotter: 主窗口类，包含用户界面和绘图逻辑

依赖项：
- pandas: 用于处理 CSV 文件
- PyQt6: 用于创建图形用户界面
- pyqtgraph: 用于绘制图表
"""

import sys
from typing import Dict, Callable
import pandas as pd
from PyQt6.QtCore import Qt
from PyQt6.QtWidgets import (
    QApplication,
    QMainWindow,
    QTabWidget,
    QWidget,
    QVBoxLayout,
    QScrollArea,
    QHBoxLayout,
    QLineEdit,
    QPushButton,
    QFileDialog,
    QLabel,
)
import pyqtgraph as pg

# 常量定义
BACKGROUND_COLOR = "#2E2E2E"  # 深灰色背景
TEXT_COLOR = "#FFFFFF"  # 白色文字
BAR_COLOR = "#3498DB"  # 蓝色条形图
HIGHLIGHT_COLOR = "#2ECC71"  # 绿色高亮
MAX_BAR_COLOR = "#E74C3C"  # 红色最大值条形图
ANNOTATION_COLOR = "#FFFFFF"  # 白色注释


STYLESHEET = """
QMainWindow {
    background-color: #2E2E2E;
    color: white;
}

QWidget {
    background-color: #2E2E2E;
    color: white;
}

QLabel {
    color: white;
}

QLineEdit {
    background-color: #2E2E2E;
    color: white;
    border: 1px solid #444;
}

QPushButton {
    background-color: #2E2E2E;
    color: white;
    border: 1px solid #444;
    border-radius: 5px;
    padding: 2px;
}
QPushButton:hover {
    background-color: #444;
}
QPushButton:pressed {
    background-color: #666;
}

QTabWidget::pane {
    border: 1px solid #444;
    background-color: #2E2E2E;
}

QTabBar::tab {
    background: #444;
    color: white;
    padding: 10px;
}

QTabBar::tab:selected {
    background: #666;
}

QScrollArea {
    background-color: #2E2E2E;
}

QScrollArea QWidget {
    background-color: #2E2E2E;
}

QScrollBar:vertical {
    background: #2E2E2E;
}

QScrollBar:horizontal {
    background: #2E2E2E;
}
"""

class CustomBarGraphItem(pg.BarGraphItem):
    def __init__(self, plot_widget:pg.PlotWidget, show_tips=True, hover_callback: Callable[[float, float], str] = None, **kwargs):
        super().__init__(**kwargs)
        self.plot_widget = plot_widget
        self.setAcceptHoverEvents(True)
        self.default_brushes = self.opts['brushes']
        self.show_tips = show_tips
        self.hover_callback = hover_callback
        self.tip_index_prev = 0
        # 创建 QLabel 作为提示
        self.label = QLabel("", self.plot_widget)
        self.label.setStyleSheet(f"""
            background-color: rgba(46, 46, 46, 180);  # 深灰色背景，半透明
            color: {TEXT_COLOR};
            border: 1px solid {TEXT_COLOR};
            border-radius: 10px;
            padding: 5px;
            text-align: center;
        """)
        self.label.setAlignment(Qt.AlignmentFlag.AlignLeft)
        self.label.setVisible(False)

        self.highlight_color = HIGHLIGHT_COLOR

        # 创建垂直线
        self.v_line = pg.InfiniteLine(angle=90, movable=False, pen=pg.mkPen(HIGHLIGHT_COLOR))
        self.plot_widget.addItem(self.v_line)
        self.v_line.setVisible(False)
        self.v_line.setPen(pg.mkPen(self.highlight_color, style=Qt.PenStyle.DashLine))

        # 创建水平线
        self.h_line = pg.InfiniteLine(angle=0, movable=False, pen=pg.mkPen(HIGHLIGHT_COLOR))
        self.plot_widget.addItem(self.h_line)
        self.h_line.setVisible(False)
        self.h_line.setPen(pg.mkPen(self.highlight_color, style=Qt.PenStyle.DashLine))

    def setTipStyle(self, background_color=None, text_color=None, font_size=None, border_radius=None):
        style = "background-color: {}; color: {}; font-size: {}; border-radius: {}; padding: 5px; text-align: center;".format(
            background_color if background_color else BACKGROUND_COLOR,
            text_color if text_color else TEXT_COLOR,
            font_size if font_size else "12px",
            border_radius if border_radius else "10px"
        )
        self.label.setStyleSheet(style)

    def setHighlightColor(self, color):
        self.highlight_color = color
        self.v_line.setPen(pg.mkPen(color, style=Qt.PenStyle.DashLine))
        self.h_line.setPen(pg.mkPen(color, style=Qt.PenStyle.DashLine))

    def hoverMoveEvent(self, event):
        pos = event.pos()
        index = self._getBarIndex(pos, True)
        if index is not None:
            self.opts['brushes'] = [pg.mkBrush(self.highlight_color) if i == index else brush for i, brush in enumerate(self.default_brushes)]
            super().setOpts(**self.opts)

            datax = self.opts["x"]
            datay = self.opts["height"]

            # 显示垂直线
            self.v_line.setPos(datax[index])
            self.v_line.setVisible(True)
            # 显示水平线
            self.h_line.setPos(datay[index])
            self.h_line.setVisible(True)

            if self.show_tips:
                if self.hover_callback:
                    tip_text = self.hover_callback(datax[index], datay[index])
                else:
                    tip_text = f"x: {datax[index]}, y: {datay[index]}"

                if self.tip_index_prev != index:
                    self.label.setVisible(False)
                    self.tip_index_prev = index

                self.label.setText(tip_text)
                self.label.setVisible(True)
                scene_pos = self.mapToScene(pos)
                view_pos = self.plot_widget.mapFromScene(scene_pos)
                self.label.move(view_pos.x() + 10, view_pos.y() - 20)  # 调整位置
        event.accept()

    def hoverLeaveEvent(self, event):
        self.opts['brushes'] = self.default_brushes
        super().setOpts(**self.opts)

        self.label.setVisible(False)
        self.v_line.setVisible(False)
        self.h_line.setVisible(False)
        event.accept()

    def updateData(self, y):
        self.opts['height'] = y
        super().setOpts(**self.opts)
        self.plot_widget.plotItem.vb.autoRange()  # 调用 viewAll 方法更新视图

    def _getBarIndex(self, pos, use_x_range=False):
        if use_x_range:
            x_pos = pos.x()
            for i, rect in enumerate(self._rectarray.instances()):
                if rect.left() <= x_pos <= rect.right():
                    return i
        else:
            for i, rect in enumerate(self._rectarray.instances()):
                if rect.contains(pos):
                    return i
        return None

class DrawCallPlotter(QMainWindow):
    def __init__(self):
        super().__init__()
        self.setWindowTitle("Draw Call Attributes Plotter")
        self.setMinimumSize(800, 600)

        # 加载样式表
        # with open("styles.qss", "r") as f:
        #     self.setStyleSheet(f.read())
        self.setStyleSheet(STYLESHEET)

        # 创建文件路径输入框和按钮
        self.file_label = QLabel("CSV文件路径:", self)

        self.file_input = QLineEdit(self)
        self.file_input.setText("pgame_city.csv")
        self.file_input.setPlaceholderText("选择CSV文件路径")

        self.browse_button = QPushButton("浏览", self)
        self.browse_button.clicked.connect(self.browse_file)

        self.load_button = QPushButton("加载", self)
        self.load_button.clicked.connect(self.load_csv)

        # 布局
        input_layout = QHBoxLayout()
        input_layout.addWidget(self.file_label)
        input_layout.addWidget(self.file_input)
        input_layout.addWidget(self.browse_button)
        input_layout.addWidget(self.load_button)

        input_widget = QWidget()
        input_widget.setLayout(input_layout)

        self.main_layout = QVBoxLayout()
        self.main_layout.addWidget(input_widget)

        self.tab_control = QTabWidget()
        self.tab_control.currentChanged.connect(self.on_tab_changed)
        self.main_layout.addWidget(self.tab_control)

        main_widget = QWidget()
        main_widget.setLayout(self.main_layout)
        self.setCentralWidget(main_widget)

        self.current_tab = None
        self.times = None

    def on_tab_changed(self, index):
        self.current_tab = self.tab_control.tabText(index)

    def browse_file(self):
        file_name, _ = QFileDialog.getOpenFileName(
            self, "选择CSV文件", "", "CSV Files (*.csv)"
        )
        if file_name:
            self.file_input.setText(file_name)
            self.load_csv()

    def load_csv(self):
        csv_file = self.file_input.text()
        if csv_file:
            self.df = pd.read_csv(csv_file)
            self.df.columns = self.df.columns.str.strip()
            self.df = self.df.dropna(subset=["ID"]).reset_index(drop=True)
            self.df["ID"] = self.df["ID"].astype(int)
            self.df = self.df.fillna(0)
            self.attributes = [
                col
                for col in self.df.columns
                if col != "ID"
                and col != "Context"
                and pd.api.types.is_numeric_dtype(self.df[col])
            ]
            self.plot_initial_charts()

    def plot_initial_charts(self):
        self.tab_control.clear()
        self.tabs: Dict[str, QVBoxLayout] = {}
        self.plot_widgets: Dict[str, pg.PlotWidget] = {}
        self.bar_graphs: Dict[str, CustomBarGraphItem] = {}

        for attribute in self.attributes:
            scroll_area = QScrollArea()
            scroll_area.setWidgetResizable(True)
            scroll_content = QWidget()
            scroll_layout = QVBoxLayout(scroll_content)
            scroll_area.setWidget(scroll_content)
            self.tab_control.addTab(scroll_area, attribute)
            self.tabs[attribute] = scroll_layout
            if attribute == "Clocks":
                self.add_frequency_input(scroll_layout, attribute)
            self.plot_chart(attribute)

    def add_frequency_input(self, layout, attribute):
        freq_label = QLabel("频率(Clocks/Second):", self)
        freq_input = QLineEdit(self)
        freq_input.setPlaceholderText("输入频率")
        freq_input.textChanged.connect(
            lambda text: self.on_freq_change(attribute, float(text)) if text else None
        )

        freq_btn = QPushButton("转换耗时(ms)", self)
        freq_btn.clicked.connect(
            lambda: self.on_convert_time(attribute, float(freq_input.text()))
        )
        freq_layout = QHBoxLayout()
        freq_layout.addWidget(freq_label)
        freq_layout.addWidget(freq_input)
        freq_layout.addWidget(freq_btn)
        layout.addLayout(freq_layout)


    def on_freq_change(self, attribute, frequency):
        self.times = self.df[attribute] / frequency * 1000  # 计算耗时并转换成毫秒

    def on_convert_time(self, attribute, frequency):
        if attribute in self.plot_widgets and attribute in self.bar_graphs:
            plot_widget = self.plot_widgets[attribute]
            bar_graph = self.bar_graphs[attribute]
            plot_widget.setLabel("left", "耗时(ms)", color=TEXT_COLOR)
            bar_graph.updateData(self.times)

    def plot_chart(self, attribute, data=None):
        if data is None:
            data = self.df[attribute]

        plot_widget = pg.PlotWidget()
        plot_widget.setBackground(BACKGROUND_COLOR)
        plot_widget.setTitle(
            f"{attribute} Per Draw Call", color=TEXT_COLOR, size="14pt"
        )
        plot_widget.showGrid(x=False, y=True)
        plot_widget.setLabel("left", attribute, color=TEXT_COLOR)
        plot_widget.setLabel("bottom", "DrawCall ID", color=TEXT_COLOR)

        # 高亮最大值
        brushes = [pg.mkBrush(BAR_COLOR) for _ in data]
        brushes[data.idxmax()] = pg.mkBrush(MAX_BAR_COLOR)

        bar_graph = CustomBarGraphItem(
            plot_widget=plot_widget,
            x=self.df["ID"], height=data, width=0.6, brushes=brushes,
            hover_callback=self.hover_callback
        )
        bar_graph.setTipStyle(background_color="rgba(46, 146, 46, 230)", font_size="15px", border_radius="10px")

        plot_widget.addItem(bar_graph)

        self.tabs[attribute].addWidget(plot_widget)
        self.plot_widgets[attribute] = plot_widget
        self.bar_graphs[attribute] = bar_graph


    # 保留小数部分的两位有效数字，整数部分保持不变
    def format_value(self,x):
        if x >= 1:
            return f"{x:.2f}".rstrip('0').rstrip('.')
        else:
            return f"{x:.2g}"

    def hover_callback(self, datax, datay):
        formatted_datay = self.format_value(datay)
        tooltip_text = f"ID: {datax}\n{formatted_datay}"
        if self.current_tab:
            if "%" in self.current_tab:
                tooltip_text += "%"
            elif "Bytes" in self.current_tab:
                tooltip_text += " Bytes"
                mb_value = datay / (1024 * 1024)
                tooltip_text += f"\n{self.format_value(mb_value)} MB"
            elif self.current_tab == "Clocks":
                tooltip_text += " Clocks"
                if self.times is not None and not self.times.empty:
                    tooltip_text += f"\n{self.format_value(self.times[datax])} ms"

        return tooltip_text


if __name__ == "__main__":
    app = QApplication(sys.argv)
    main_window = DrawCallPlotter()
    main_window.show()
    sys.exit(app.exec())

