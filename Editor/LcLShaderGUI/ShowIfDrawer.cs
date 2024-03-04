using UnityEngine;
using UnityEditor;
using System;

namespace LcLShaderEditor
{
    /// <summary>
    /// 根据条件决定是否绘制属性GUI
    /// Usage:
    /// [Toggle(_SWITCH)] _SWITCH ("Toggle", int) = 0
    /// [ShowIf(_SWITCH, 1)]_Color("Color", Color) = (1, 1, 1, 1)
    /// [ShowIf(_SWITCH, 0)]_Tex("Texture", 2D) = "white" { }
    /// </summary>
    public class ShowIfDrawer : MaterialPropertyDrawer
    {
        string m_PropertyName;
        float m_ConditionValue;
        bool m_Condition;

        public ShowIfDrawer(string toggleName)
        {
            m_PropertyName = toggleName;
            m_ConditionValue = 1;
        }
        public ShowIfDrawer(string toggleName, float value)
        {
            m_PropertyName = toggleName;
            m_ConditionValue = value;
        }

        override public void OnGUI(Rect pos, MaterialProperty prop, string label, MaterialEditor editor)
        {
            var mat = prop.targets[0] as Material;
            m_Condition = mat.GetFloat(m_PropertyName) == m_ConditionValue;
            if (m_Condition)
            {
                editor.DefaultShaderProperty(prop, label);
            }
        }
        override public float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            // return condition ? base.GetPropertyHeight(prop, label, editor) : 0;
            return 0;
        }
    }
}
