using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace LcLShaderEditor
{
    public static class ShaderEditorHandler
    {
        private static Assembly m_UnityEditor_Assembly = Assembly.GetAssembly(typeof(Editor));
        private static Type m_MaterialPropertyHandler_Type = m_UnityEditor_Assembly.GetType("UnityEditor.MaterialPropertyHandler");
        private static MethodInfo m_GetShaderPropertyDrawer_Method = m_MaterialPropertyHandler_Type.GetMethod("GetShaderPropertyDrawer", BindingFlags.Static | BindingFlags.NonPublic);

        delegate MaterialPropertyDrawer GetShaderPropertyDrawerType(string attrib, out bool isDecorator);

        static GetShaderPropertyDrawerType m_GetShaderPropertyDrawerType;

        public static MaterialPropertyDrawer GetShaderPropertyDrawer(string attrib, out bool isDecorator)
        {
            if (m_GetShaderPropertyDrawerType == null)
            {
                Assembly editorAssembly = typeof(Editor).Assembly;
                Type handlerType = editorAssembly.GetType("UnityEditor.MaterialPropertyHandler");
                MethodInfo method = handlerType.GetMethod("GetShaderPropertyDrawer", BindingFlags.Static | BindingFlags.NonPublic);
                m_GetShaderPropertyDrawerType = Delegate.CreateDelegate(typeof(GetShaderPropertyDrawerType), method) as GetShaderPropertyDrawerType;
            }

            return m_GetShaderPropertyDrawerType(attrib, out isDecorator) as MaterialPropertyDrawer;
        }


        // -------------------------------Foldout GUI-----------------------------------------

        static GUIStyle m_GuiStyleFoldout;

        static GUIStyle GuiStyleFoldout
        {
            get
            {
                if (m_GuiStyleFoldout == null)
                {
                    m_GuiStyleFoldout = new GUIStyle("minibutton")
                    {
                        contentOffset = new Vector2(22, 0),
                        fixedHeight = 20,
                        alignment = TextAnchor.MiddleLeft,
                        font = EditorStyles.boldLabel.font,
                        fontSize = EditorStyles.boldLabel.fontSize
#if UNITY_2019_4_OR_NEWER
                                   + 1,
#endif
                    };
                }

                return m_GuiStyleFoldout;
            }
        }

        static GUIStyle m_ToggleStyle;

        static GUIStyle ToggleStyle
        {
            get
            {
                if (m_ToggleStyle == null) m_ToggleStyle = new GUIStyle("Toggle");
                return m_ToggleStyle;
            }
        }

        static GUIStyle m_ToggleMixedStyle;

        static GUIStyle ToggleMixedStyle
        {
            get
            {
                if (m_ToggleMixedStyle == null) m_ToggleMixedStyle = new GUIStyle("ToggleMixed");
                return m_ToggleMixedStyle;
            }
        }

        public static void Foldout( Rect rect, bool isFolding, string title, bool hasToggle, bool toggleValue, Action<bool> foldoutAction, Action<bool> toggleAction)
        {
            var toggleRect = new Rect(rect.x + 8f, rect.y + 3f, 13f, 13f);
            // rect = GUILayoutUtility.GetRect(16f, 25f);
            // Toggle Event
            if (hasToggle)
            {
                if (Event.current.type == EventType.MouseDown
                    && Event.current.button == 0
                    && toggleRect.Contains(Event.current.mousePosition))
                {
                    toggleAction(!toggleValue);
                    Event.current.Use();
                    GUI.changed = true;
                }
            }

            // Button
            {
                // Right Click to Context Click
                if (Event.current.type == EventType.MouseDown && Event.current.button == 1 && rect.Contains(Event.current.mousePosition))
                    Event.current.Use();

                var enabled = GUI.enabled;
                GUI.enabled = true;
                var guiColor = GUI.backgroundColor;
                GUI.backgroundColor = isFolding ? Color.white : new Color(0.85f, 0.85f, 0.85f);
                if (GUI.Button(rect, title, GuiStyleFoldout))
                {
                    foldoutAction(!isFolding);
                }

                GUI.backgroundColor = guiColor;
                GUI.enabled = enabled;
            }

            // Toggle Icon
            if (hasToggle)
            {
                GUI.Toggle(toggleRect, !EditorGUI.showMixedValue && toggleValue, String.Empty,
                    EditorGUI.showMixedValue ? ToggleMixedStyle : ToggleStyle);
            }
        }


        public static readonly float revertButtonWidth = 20;
        public static readonly float space = 5;

        public static Rect SplitRevertButtonRect(ref Rect rect)
        {
            Rect buttonRect = new Rect(rect.x + rect.width - revertButtonWidth, rect.y, revertButtonWidth, rect.height);
            rect.width -= (revertButtonWidth + space);
            return buttonRect;
        }

        public static void DrawPropertyRevertButton(Rect rect, Action revertAction)
        {
            if (GUI.Button(rect, EditorGUIUtility.IconContent("d_Refresh"), GUIStyle.none))
            {
                revertAction();
            }
        }

        // ---------------------------------Data存储-------------------------------------------------
        public static bool GetFoldoutState(int instanceId, string propName)
        {
            var state = PlayerPrefs.GetInt($"{instanceId}_{propName}",1);
            return state == 1;
        }
        public static bool GetFoldoutState(Material mat, string propName)
        {
            var instanceId = mat.GetInstanceID();
            return GetFoldoutState(instanceId, propName);
        }
        public static void SetFoldoutState(Material mat, string propName, bool state)
        {
            var instanceId = mat.GetInstanceID();
            PlayerPrefs.SetInt($"{instanceId}_{propName}", state ? 1 : 0);
        }


        // ---------------------------------Property Data-------------------------------------------------
        //已弃用,效率太低
        public static SerializedProperty GetProperty(this SerializedObject serializedObject, string name)
        {
            var serializedProperty = serializedObject.FindProperty("m_SavedProperties.m_Ints");
            SerializedProperty element = null;
            foreach (SerializedProperty p in serializedProperty)
            {
                if (p.FindPropertyRelative("first").stringValue == name)
                {
                    element = p;
                    break;
                }
            }

            if (element == null)
            {
                serializedProperty.InsertArrayElementAtIndex(0);
                element = serializedProperty.GetArrayElementAtIndex(0);
                element.FindPropertyRelative("first").stringValue = name;
                element.FindPropertyRelative("second").intValue = 1;
                serializedObject.ApplyModifiedProperties();
            }

            return element;
        }

        /// <summary>
        /// 获取SerializedProperty的Int属性
        /// </summary>
        /// <returns></returns>
        public static int GetPropertyIntValue(this SerializedProperty property)
        {
            return property.FindPropertyRelative("second").intValue;
        }
        public static void SetPropertyIntValue(this SerializedProperty property, string name, int value)
        {
            property.FindPropertyRelative("second").intValue = value;
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}