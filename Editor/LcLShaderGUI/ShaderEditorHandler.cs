using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
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
        public static bool Foldout(Rect rect, bool isFolding, string title, bool hasToggle, ref bool toggleValue)
        {
            var toggleRect = new Rect(rect.x + 8f, rect.y + 5f, 13f, 13f);
            rect = GUILayoutUtility.GetRect(16f, 25f);
            // Toggle Event
            if (hasToggle)
            {
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && toggleRect.Contains(Event.current.mousePosition))
                {
                    toggleValue = !toggleValue;
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
                    isFolding = !isFolding;
                }
                GUI.backgroundColor = guiColor;
                GUI.enabled = enabled;
            }

            // Toggle Icon
            if (hasToggle)
            {
                GUI.Toggle(toggleRect, EditorGUI.showMixedValue ? false : toggleValue, String.Empty,
                           EditorGUI.showMixedValue ? ToggleMixedStyle : ToggleStyle);
            }

            return isFolding;
        }


        // ----------------------------------------------------------------------------------

        // public static string foldoutFlag = "_FoldoutValue";
        public static string foldoutFlag = "_FoldoutState";
        public static string GetFoldoutPropName(string propName)
        {
            return propName + foldoutFlag;
        }
        /// <summary>
        /// 获取材质的Float属性
        /// </summary>
        /// <param name="serializedObject">new SerializedObject(mat)</param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static float GetHiddenPropertyFloat(this SerializedObject serializedObject, string name)
        {
            var serializedProperty = serializedObject.FindProperty("m_SavedProperties.m_Floats");

            SerializedProperty element = null;
            for (int i = 0; i < serializedProperty.arraySize; i++)
            {
                SerializedProperty p = serializedProperty.GetArrayElementAtIndex(i);
                if (p.FindPropertyRelative("first").stringValue == name)
                {
                    element = p;
                    break;
                }
            }

            if (element == null)
            {
                serializedProperty.InsertArrayElementAtIndex(1);
                element = serializedProperty.GetArrayElementAtIndex(1);
                element.FindPropertyRelative("first").stringValue = name;
                element.FindPropertyRelative("second").floatValue = 1.0f;
                serializedObject.ApplyModifiedProperties();
                // AssetDatabase.SaveAssets();
            }

            return element.FindPropertyRelative("second").floatValue;
        }

        public static void SetHiddenPropertyFloat(this SerializedObject serializedObject, string name, float value)
        {
            var serializedProperty = serializedObject.FindProperty("m_SavedProperties.m_Floats");

            SerializedProperty element = null;
            for (int i = 0; i < serializedProperty.arraySize; i++)
            {
                SerializedProperty p = serializedProperty.GetArrayElementAtIndex(i);
                if (p.FindPropertyRelative("first").stringValue == name)
                {
                    element = p;
                    break;
                }
            }

            if (element == null)
            {
                serializedProperty.InsertArrayElementAtIndex(1);
                element = serializedProperty.GetArrayElementAtIndex(1);
                element.FindPropertyRelative("first").stringValue = name;
                element.FindPropertyRelative("second").floatValue = value;
                serializedProperty.serializedObject.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
            }
            else
            {
                element.FindPropertyRelative("second").floatValue = value;
                serializedProperty.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
