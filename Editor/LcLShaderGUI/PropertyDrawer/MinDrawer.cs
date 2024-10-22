using UnityEngine;
using UnityEditor;

namespace LcLShaderEditor
{
    /// <summary>
    /// 限制属性的最小值
    /// Usage:
    /// [Min(1)]_Test ("Test", float) = 0
    /// [Min(1,2,3)]_Test ("Test2", Vector) =  (0, 0, 0, 0)
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
            var type = prop.type;
            if (type == MaterialProperty.PropType.Float)
            {
                var floatValue = Mathf.Max(m_Min.x, prop.floatValue);
                EditorGUI.BeginChangeCheck();
                floatValue = EditorGUI.FloatField(pos, label, floatValue);
                if (EditorGUI.EndChangeCheck())
                {
                    prop.floatValue = floatValue;
                }
            }
            else if (type == MaterialProperty.PropType.Int)
            {
                var intValue = Mathf.Max((int)m_Min.x, prop.intValue);
                EditorGUI.BeginChangeCheck();
                intValue = EditorGUI.IntField(pos, label, intValue);
                if (EditorGUI.EndChangeCheck())
                {
                    prop.intValue = intValue;
                }
            }
            else if (type == MaterialProperty.PropType.Vector)
            {
                Vector4 v = prop.vectorValue;
                if (m_Min.x != float.MinValue) v.x = Mathf.Max(m_Min.x, v.x);
                if (m_Min.y != float.MinValue) v.y = Mathf.Max(m_Min.y, v.y);
                if (m_Min.z != float.MinValue) v.z = Mathf.Max(m_Min.z, v.z);
                if (m_Min.w != float.MinValue) v.w = Mathf.Max(m_Min.w, v.w);

                EditorGUI.BeginChangeCheck();
                {
                    EditorGUI.showMixedValue = prop.hasMixedValue;
                    var oldLabelWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = 0f;

                    v = EditorGUI.Vector4Field(pos, label, v);

                    EditorGUIUtility.labelWidth = oldLabelWidth;
                    EditorGUI.showMixedValue = false;
                }
                if (EditorGUI.EndChangeCheck()) prop.vectorValue = v;
            }
            else
            {
                editor.DefaultShaderProperty(prop, label);
            }
        }
    }
}