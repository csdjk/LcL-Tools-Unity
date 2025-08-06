using System;
using System.Globalization;
using LcLTools.UnityToolbarExtender;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

namespace LcLTools
{
    public class ColorPickerWindow : EditorWindow
    {
        public static bool isPickingColor = false;

        [MenuItem("LcLTools/ColorPicker")]
        public static void ShowWindow()
        {
            isPickingColor = false;
            EditorWindow.GetWindow<ColorPickerWindow>("ColorPicker");
        }

        private string m_ColorLuminance = "1";
        private string m_ColorRGB = "1f, 1f, 1f, 1f";

        private string m_LinearColorRGB = "1, 1, 1, 1";
        private string m_LinearLuminance = "1";

        private string m_ColorRGB32 = "255, 255, 255, 255";
        private string m_ColorHex = "FFFFFFFF";
        private string m_ColorHSV = "100, 100, 100, 100";
        private Color m_Color = new Color(1, 1, 1, 1);

        void OnGUI()
        {
            EditorGUIUtility.labelWidth = 80;

            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.ColorField(new GUIContent("Color"), m_Color, false, true, false,
                    GUILayout.ExpandWidth(true));
                var icon = EditorGUIUtility.IconContent("Grid.PickingTool").image;
                if (GUILayout.Button(new GUIContent("", icon), GUILayout.Width(25)))
                {
                    isPickingColor = true;
                    EyeDropper.Start(GUIView.current);
                    GUIUtility.ExitGUI();
                }
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.TextField("SRGB:", m_ColorRGB);
                // EditorGUILayout.Space();
                EditorGUILayout.TextField("Luminance:", m_ColorLuminance);
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.TextField("LinearRGB:", m_LinearColorRGB);
                // EditorGUILayout.Space(5);
                EditorGUILayout.TextField("LinearLum:", m_LinearLuminance);
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.TextField("RGB32:", m_ColorRGB32);
            EditorGUILayout.Space();
            EditorGUILayout.TextField("Hex:", m_ColorHex);
            EditorGUILayout.Space();
            EditorGUILayout.TextField("HSV:", m_ColorHSV);

            // 检测按键事件
            Event e = Event.current;
            if (Event.current.commandName == "EyeDropperClicked")
            {
                isPickingColor = false;
                EyeDropper.End();
                Debug.Log("Color picking canceled.");
            }

            if (isPickingColor)
            {
                m_Color = EyeDropper.GetPickedColor();
                UpdateColor();
                //刷新界面
                Repaint();
            }
        }


        /// <summary>
        /// 更新颜色值
        /// </summary>
        private void UpdateColor()
        {
            m_ColorHex = ColorUtility.ToHtmlStringRGBA(m_Color);

            m_ColorRGB = $"{m_Color.r}, {m_Color.g}, {m_Color.b}, {m_Color.a}";

            var linearColor = m_Color.linear;
            m_LinearColorRGB = $"{linearColor.r}, {linearColor.g}, {linearColor.b}, {linearColor.a}";

            Color32 color32 = m_Color;
            m_ColorRGB32 = $"{color32.r}, {color32.g}, {color32.b}, {color32.a}";

            float h, s, v;
            Color.RGBToHSV(m_Color, out h, out s, out v);
            m_ColorHSV = $"{h}, {s}, {v}";

            m_ColorLuminance = v.ToString(CultureInfo.CurrentCulture);

            Color.RGBToHSV(linearColor, out h, out s, out v);
            m_LinearLuminance = v.ToString(CultureInfo.CurrentCulture);
        }

        protected void OnDestroy()
        {
            if (isPickingColor)
            {
                isPickingColor = false;
                EyeDropper.End();
            }
        }
    }


    [InitializeOnLoad]
    public class ColorPicker
    {
        static ColorPicker()
        {
            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
        }

        static void OnToolbarGUI()
        {
            var style = new GUIStyle(EditorStyles.toolbarButton);
            style.alignment = TextAnchor.MiddleCenter;
            var icon = EditorGUIUtility.IconContent("Grid.PickingTool").image;
            if (GUILayout.Button(new GUIContent("", icon), style, GUILayout.Width(25)))
            {
                ColorPickerWindow.ShowWindow();
            }
        }
    }
}
