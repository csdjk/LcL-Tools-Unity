using UnityEngine;
using UnityEditor;
using System;

namespace LcLShaderEditor
{
    /// <summary>
    /// 限制属性的最小值
    /// Usage:
    /// [Min(0)]_Test ("Test", float) = 0
    /// [Min(0,1,2)]_Test ("Test2", Vector) =  (0, 0, 0, 0)
    /// </summary>
    public class MinDrawer : MaterialPropertyDrawer
    {
        Vector4 m_Min = new Vector4(float.MinValue, float.MinValue, float.MinValue, float.MinValue);

        public MinDrawer(float x)
        {
            m_Min.x = x;
        }

        public MinDrawer(float minx, float miny)
        {
            m_Min.x = minx;
            m_Min.y = miny;
        }

        public MinDrawer(float minx, float miny, float minz)
        {
            m_Min.x = minx;
            m_Min.y = miny;
            m_Min.z = minz;
        }

        public MinDrawer(float minx, float miny, float minz, float minw)
        {
            m_Min.x = minx;
            m_Min.y = miny;
            m_Min.z = minz;
            m_Min.w = minw;
        }

        override public void OnGUI(Rect pos, MaterialProperty prop, string label, MaterialEditor editor)
        {
            var mat = prop.targets[0] as Material;
            if (mat == null)
            {
                return;
            }

            if (prop.type == MaterialProperty.PropType.Float)
            {
                prop.floatValue = Mathf.Max(m_Min.x, prop.floatValue);
            }
            else if (prop.type == MaterialProperty.PropType.Int)
            {
                prop.intValue = Mathf.Max((int)m_Min.x, prop.intValue);
            }
            else if (prop.type == MaterialProperty.PropType.Vector)
            {
                Vector4 v = prop.vectorValue;
                if (m_Min.x != float.MinValue) v.x = Mathf.Max(m_Min.x, v.x);
                if (m_Min.y != float.MinValue) v.y = Mathf.Max(m_Min.y, v.y);
                if (m_Min.z != float.MinValue) v.z = Mathf.Max(m_Min.z, v.z);
                if (m_Min.w != float.MinValue) v.w = Mathf.Max(m_Min.w, v.w);
                prop.vectorValue = v;
            }

            editor.DefaultShaderProperty(prop, label);
        }

        override public float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return 0;
        }
    }
}