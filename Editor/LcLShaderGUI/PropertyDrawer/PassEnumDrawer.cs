using UnityEngine;
using UnityEditor;
using System;

namespace LcLShaderEditor
{
    /// <summary>
    /// 初始化并禁用 Shader Pass
    /// </summary>
    public class InitDisablePassDrawer : MaterialPropertyDrawer
    {
        string m_ShaderPassName;
        public InitDisablePassDrawer(string name)
        {
            m_ShaderPassName = name;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            var material = prop.targets[0] as Material;

            if (prop.floatValue == 0)
            {
                material.SetShaderPassEnabled(m_ShaderPassName, false);
                prop.floatValue = 1;
            }

            if (material.GetShaderPassEnabled(m_ShaderPassName))
            {
                GUILayout.Label($"Disabled Shader Pass: {m_ShaderPassName}");
            }
        }

        override public float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return 0;
        }
    }
    /// <summary>
    /// Shader Pass 开关
    /// </summary>
    public class EnablePassDrawer : MaterialPropertyDrawer
    {
        string[] m_PassList;
        public EnablePassDrawer(params string[] name)
        {
            m_PassList = name;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            var material = prop.targets[0] as Material;
            var passEnabled = material.GetShaderPassEnabled(m_PassList[0]);
            passEnabled = EditorGUILayout.Toggle(label, passEnabled);
            foreach (var pass in m_PassList)
            {
                material.SetShaderPassEnabled(pass, passEnabled);
            }
        }

        override public float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return 0;
        }
    }

    /// <summary>
    /// Shader Pass枚举
    /// </summary>
    public class PassEnumDrawer : MaterialPropertyDrawer
    {
        string[] m_ShaderPassNames;
        public PassEnumDrawer(params string[] args)
        {
            m_ShaderPassNames = args;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            var material = prop.targets[0] as Material;

            // draw enum
            var index = (int)prop.floatValue;
            index = EditorGUILayout.Popup(label, index, m_ShaderPassNames);
            prop.floatValue = index;

            for (var i = 0; i < m_ShaderPassNames.Length; i++)
            {
                var shaderPassName = m_ShaderPassNames[i];
                material.SetShaderPassEnabled(shaderPassName, i == index);
            }
        }
        override public float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return 0;
        }
    }
}
