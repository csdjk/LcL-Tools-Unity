using UnityEngine;
using UnityEditor;
using System;

namespace LcLShaderEditor
{
    public enum SurfaceType
    {
        Opaque,
        Transparent
    }

    /// <summary>
    /// Texture的简单面板
    /// </summary>
    class SingleLineDrawer : MaterialPropertyDrawer
    {
        float m_Height;

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            m_Height = position.height;
            editor.TexturePropertySingleLine(label, prop);
        }
        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return m_Height;
        }
    }
}
