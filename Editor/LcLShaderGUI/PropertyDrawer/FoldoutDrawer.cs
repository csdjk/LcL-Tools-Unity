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
        Material m_Mat;
        bool IsKeyword => m_Keyword != null;

        public FoldoutDrawer()
        {
        }
        public FoldoutDrawer(string keyword)
        {
            m_Keyword = keyword;
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
            var foldoutState = ShaderEditorHandler.GetFoldoutState(m_Mat, prop.name);
            var toggleValue = prop.floatValue > 0;
            ShaderEditorHandler.Foldout(position, foldoutState, label.text, IsKeyword, toggleValue, (v) =>
            {
                ShaderEditorHandler.SetFoldoutState(m_Mat, prop.name, v);
            }, (v) =>
            {
                prop.floatValue = Convert.ToSingle(v);
                SetKeyword(m_Mat, v);
            });
        }
        //
        // public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        // {
        //     return 0;
        // }
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