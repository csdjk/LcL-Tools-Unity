using UnityEngine;
using UnityEditor;

namespace LcLShaderEditor
{
    /// <summary>
    /// 限制属性的最大值
    /// Usage:
    /// [Max(1)]_Test ("Test", float) = 0
    /// [Max(1,2,3)]_Test ("Test2", Vector) =  (0, 0, 0, 0)
    /// </summary>
    public class MaxDrawer : MaterialPropertyDrawer
    {
        Vector4 m_Max = new Vector4(float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue);

        public MaxDrawer(float x)
        {
            m_Max.x = x;
        }

        public MaxDrawer(float maxx, float maxy)
        {
            m_Max.x = maxx;
            m_Max.y = maxy;
        }

        public MaxDrawer(float maxx, float maxy, float maxz)
        {
            m_Max.x = maxx;
            m_Max.y = maxy;
            m_Max.z = maxz;
        }

        public MaxDrawer(float maxx, float maxy, float maxz, float maxw)
        {
            m_Max.x = maxx;
            m_Max.y = maxy;
            m_Max.z = maxz;
            m_Max.w = maxw;
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
                prop.floatValue = Mathf.Min(m_Max.x, prop.floatValue);
            }
            else if (prop.type == MaterialProperty.PropType.Int)
            {
                prop.intValue = Mathf.Min((int)m_Max.x, prop.intValue);
            }
            else if (prop.type == MaterialProperty.PropType.Vector)
            {
                Vector4 v = prop.vectorValue;
                if (m_Max.x != float.MaxValue) v.x = Mathf.Min(m_Max.x, v.x);
                if (m_Max.y != float.MaxValue) v.y = Mathf.Min(m_Max.y, v.y);
                if (m_Max.z != float.MaxValue) v.z = Mathf.Min(m_Max.z, v.z);
                if (m_Max.w != float.MaxValue) v.w = Mathf.Min(m_Max.w, v.w);
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