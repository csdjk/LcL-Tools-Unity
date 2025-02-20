# 这个脚本是一个用于合并立方体贴图的工具。
# 允许用户选择六个立方体贴图面（EXR 格式），并将它们合并成一个单一的 EXR 文件。
# 主要功能包括：
# 1. 选择立方体贴图面文件。
# 2. 验证所选文件的有效性（检查文件名和面数）。
# 3. 合并立方体贴图面并保存为一个新的 EXR 文件。
# 该脚本依赖于 OpenEXR、numpy、PIL 和 PyQt6 库。


import os
import re
import sys
import OpenEXR
import numpy as np
from PyQt6.QtWidgets import (QApplication, QMainWindow, QWidget, QVBoxLayout,
                             QPushButton, QListWidget, QFileDialog, QLabel,
                             QStatusBar)
from PyQt6.QtCore import Qt, QSize
from PIL import Image
import Imath

FACE_PATTERN = re.compile(r"(X[\+±-]|Y[\+±-]|Z[\+±-])", re.IGNORECASE)



class CubemapMerger(QMainWindow):
    def __init__(self):
        super().__init__()
        self.initUI()
        self.selected_files = []
        self.face_mapping = {
            'X+': 0, 'X-': 1,
            'Y+': 2, 'Y-': 3,
            'Z+': 4, 'Z-': 5
        }

    def initUI(self):
        self.setWindowTitle("Cubemap Merger")
        self.setMinimumSize(QSize(600, 400))

        main_widget = QWidget()
        layout = QVBoxLayout()

        self.btn_select = QPushButton("Select Cubemap Faces")
        self.btn_select.clicked.connect(self.select_files)
        layout.addWidget(self.btn_select)

        self.file_list = QListWidget()
        layout.addWidget(self.file_list)

        self.btn_merge = QPushButton("Merge Cubemap")
        self.btn_merge.clicked.connect(self.merge_cubemap)
        layout.addWidget(self.btn_merge)

        self.status = QStatusBar()
        self.setStatusBar(self.status)

        main_widget.setLayout(layout)
        self.setCentralWidget(main_widget)

    def select_files(self):
        files, _ = QFileDialog.getOpenFileNames(
            self, "Select Cubemap Faces", "",
            "EXR Images (*.exr);;All Files (*)"
        )
        if files:
            self.selected_files = files
            self.file_list.clear()
            self.file_list.addItems([os.path.basename(f) for f in files])

    def validate_files(self, files):
        faces = set()
        for f in files:
            match = FACE_PATTERN.search(os.path.basename(f))
            if not match:
                return False, f"Invalid filename: {os.path.basename(f)}"
            face = match.group(1).upper()
            if face in faces:
                return False, f"Duplicate face: {face}"
            faces.add(face)
        return len(faces) == 6, "Missing faces" if len(faces) <6 else ""

    def merge_cubemap(self):
        if not self.selected_files:
            self.status.showMessage("No files selected!")
            return
    
        valid, msg = self.validate_files(self.selected_files)
        if not valid:
            self.status.showMessage(f"Validation failed: {msg}")
            return
    
        try:
            face_images = {}
            base_size = None
            for f in self.selected_files:
                face = FACE_PATTERN.search(os.path.basename(f)).group(1).upper()
                exr_file = OpenEXR.InputFile(f)
                dw = exr_file.header()['dataWindow']
                size = (dw.max.x - dw.min.x + 1, dw.max.y - dw.min.y + 1)
                
                if not base_size:
                    base_size = size
                elif size != base_size:
                    raise ValueError(f"Size mismatch: {face} has {size}, expected {base_size}")
    
                channels = {}
                for c in ['R', 'G', 'B']:
                    channel_data = exr_file.channel(c, Imath.PixelType(Imath.PixelType.FLOAT))
                    channels[c] = np.frombuffer(channel_data, dtype=np.float32).reshape(size[1], size[0])
                face_images[face] = channels
    
            # Create output EXR
            output_header = OpenEXR.Header(base_size[0]*4, base_size[1]*3)
            output_header['channels'] = {'R': Imath.Channel(Imath.PixelType(OpenEXR.HALF)),
                                        'G': Imath.Channel(Imath.PixelType(OpenEXR.HALF)),
                                        'B': Imath.Channel(Imath.PixelType(OpenEXR.HALF))}
    
            # Define the order of faces based on the required layout
            ordered_faces = ['Y+', 'X-', 'Z+', 'X+', 'Z-', 'Y-']
            face_positions = {
                'Y+': (1, 0),
                'X-': (0, 1),
                'Z+': (1, 1),
                'X+': (2, 1),
                'Z-': (3, 1),
                'Y-': (1, 2)
            }
    
            output_data = {c: np.zeros((base_size[1]*3, base_size[0]*4), dtype=np.float16) for c in ['R', 'G', 'B']}
    
            for face in ordered_faces:
                x_offset, y_offset = face_positions[face]
                for c in ['R', 'G', 'B']:
                    output_data[c][y_offset*base_size[1]:(y_offset+1)*base_size[1], x_offset*base_size[0]:(x_offset+1)*base_size[0]] = face_images[face][c]
    
            combined_data = {c: output_data[c].tobytes() for c in ['R', 'G', 'B']}
    
            output_dir = os.path.dirname(self.selected_files[0])
            output_path = os.path.join(output_dir, "combined_cubemap.exr")
            output_file = OpenEXR.OutputFile(output_path, output_header)
            output_file.writePixels(combined_data)
    
            self.status.showMessage(f"Successfully saved to {output_path}")
        except Exception as e:
            self.status.showMessage(f"Error: {str(e)}")

if __name__ == "__main__":
    app = QApplication(sys.argv)
    window = CubemapMerger()
    window.show()
    sys.exit(app.exec())
