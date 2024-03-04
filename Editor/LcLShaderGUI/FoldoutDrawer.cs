using UnityEngine;
using UnityEditor;
using System;

namespace LcLShaderEditor
{
    /// <summary>
    /// 文件夹
    /// </summary>
    public class FoldoutDrawer : MaterialPropertyDrawer
    {
        string m_Keyword;
        string m_FoldoutValueName;
        Material m_Mat;
        bool IsKeyword => m_Keyword != null;

        public FoldoutDrawer()
        {
        }

        public FoldoutDrawer(string keyword)
        {
            this.m_Keyword = keyword;
            m_FoldoutValueName = ShaderEditorHandler.GetFoldoutPropName(keyword);
        }

        void SetKeyword(Material material, bool on)
        {
            if (on)
                material.EnableKeyword(m_Keyword);
            else
                material.DisableKeyword(m_Keyword);
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            m_Mat = prop.targets[0] as Material;
            if (m_FoldoutValueName == null)
            {
                m_FoldoutValueName = ShaderEditorHandler.GetFoldoutPropName(prop.name);
            }

            var serializedObject = new SerializedObject(m_Mat);

            var foldoutValue = serializedObject.GetHiddenPropertyFloat(m_FoldoutValueName);
            var foldout = foldoutValue > 0;
            var toggleValue = prop.floatValue > 0;
            foldout = ShaderEditorHandler.Foldout(position, foldout, label.text, IsKeyword, ref toggleValue);

            prop.floatValue = Convert.ToSingle(toggleValue);
            serializedObject.SetHiddenPropertyFloat(m_FoldoutValueName, Convert.ToSingle(foldout));

            if (IsKeyword)
            {
                SetKeyword(m_Mat, toggleValue);
            }
            serializedObject.Dispose();
        }
        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return 0;
        }

    }

    /// <summary>
    /// Foldout End
    /// </summary>
    public class FoldoutEndDrawer : MaterialPropertyDrawer
    {
        bool m_Condition;
        float m_Height;
        public FoldoutEndDrawer(string foldout)
        {
        }
        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            m_Height = position.height;
            editor.DefaultShaderProperty(prop, label.text);
        }
        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return m_Condition ? m_Height : -2;
        }
    }

}
