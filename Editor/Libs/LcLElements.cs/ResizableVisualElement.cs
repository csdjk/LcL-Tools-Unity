
using System.CodeDom.Compiler;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections;
namespace LcLTools
{
    /// <summary>
    /// 可以调整大小的VisualElement
    /// </summary>
    public class ResizableVisualElement : VisualElement
    {
        private const string stylesResource = "Assets/Editor/Render/Elements/UIElementsExtend.uss";
        // private const string ussContainer = "resizable-box-container";
        private const string ussResizeHandleHorizontal = "resizable-box-resize-handle-horizontal";
        private const string ussResizeHandleVertical = "resizable-box-resize-handle-vertical";

        private VisualElement _content;
        private Dictionary<ResizableDir, VisualElement> _resizeHandleDict = new Dictionary<ResizableDir, VisualElement>();
        private float handleSize = 3f;

        public enum ResizableDir
        {
            Left,
            Right,
            Top,
            Bottom,
        }

        public ResizableVisualElement(float handleSize) : this()
        {
            this.handleSize = handleSize;
        }

        public ResizableVisualElement()
        {
            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(stylesResource));
            // this.AddToClassList(ussContainer);
            _content = new VisualElement();
            this.Add(_content);

            this.style.paddingLeft = handleSize;
            this.style.paddingRight = handleSize;
            this.style.paddingTop = handleSize;
            this.style.paddingBottom = handleSize;
        }


        // create resize handle
        public void AddResizeHandle(ResizableDir dir)
        {
            if (_resizeHandleDict.ContainsKey(dir))
            {
                return;
            }
            var resizeHandle = new VisualElement();
            resizeHandle.userData = dir;
            resizeHandle.RegisterCallback<MouseDownEvent>(OnMouseDownEvent);
            resizeHandle.RegisterCallback<MouseMoveEvent>(OnMouseMoveEvent);
            resizeHandle.RegisterCallback<MouseUpEvent>(OnMouseUpEvent);

            switch (dir)
            {
                case ResizableDir.Left:
                    resizeHandle.AddToClassList(ussResizeHandleHorizontal);
                    resizeHandle.style.left = 0;
                    resizeHandle.style.height = Length.Percent(100);
                    resizeHandle.style.width = handleSize;
                    break;
                case ResizableDir.Right:
                    resizeHandle.AddToClassList(ussResizeHandleHorizontal);
                    resizeHandle.style.right = 0;
                    resizeHandle.style.height = Length.Percent(100);
                    resizeHandle.style.width = handleSize;
                    break;
                case ResizableDir.Top:
                    resizeHandle.AddToClassList(ussResizeHandleVertical);
                    resizeHandle.style.top = 0;
                    resizeHandle.style.width = Length.Percent(100);
                    resizeHandle.style.height = handleSize;
                    break;
                case ResizableDir.Bottom:
                    resizeHandle.AddToClassList(ussResizeHandleVertical);
                    resizeHandle.style.bottom = 0;
                    resizeHandle.style.width = Length.Percent(100);
                    resizeHandle.style.height = handleSize;
                    break;
            }


            this.Add(resizeHandle);
            _resizeHandleDict.Add(dir, resizeHandle);
        }

        private void OnMouseDownEvent(MouseDownEvent evt)
        {
            var handle = evt.currentTarget as VisualElement;
            handle.CaptureMouse();
        }

        private void OnMouseMoveEvent(MouseMoveEvent evt)
        {
            var handle = evt.currentTarget as VisualElement;

            if (handle.HasMouseCapture())
            {
                var delta = evt.mouseDelta;
                var dir = (ResizableDir)handle.userData;

                if (dir == ResizableDir.Left || dir == ResizableDir.Right)
                {
                    this.style.width = this.layout.width + delta.x;
                }
                else if (dir == ResizableDir.Top || dir == ResizableDir.Bottom)
                {
                    this.style.height = this.layout.height + delta.y;
                }
            }
        }
        private void OnMouseUpEvent(MouseUpEvent evt)
        {
            var handle = evt.currentTarget as VisualElement;
            handle.ReleaseMouse();
        }
        // private new void Add(VisualElement content)
        // {
        //     _content.Add(content);
        // }


        public void AddChild(VisualElement content)
        {
            _content.Add(content);
        }
    }
}