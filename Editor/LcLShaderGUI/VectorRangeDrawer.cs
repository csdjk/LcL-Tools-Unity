using UnityEngine;
using UnityEditor;
using System;

namespace LcLShaderEditor
{
    public class VectorRangeDrawer : MaterialPropertyDrawer
    {
        float m_Height;

        private Vector4 m_Min;
        private Vector4 m_Max;
        public VectorRangeDrawer(float min, float max)
        {
            this.m_Min.Set(min, min, min, min);
            this.m_Max.Set(max, max, max, max);
        }
        public VectorRangeDrawer(float min0, float max0, float min1, float max1, float min2, float max2, float min3, float max3)
        {
            this.m_Min.Set(min0, min1, min2, min3);
            this.m_Max.Set(max0, max1, max2, max3);
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            m_Height = position.height;
            if (prop.type == MaterialProperty.PropType.Vector)
            {
                EditorGUIUtility.labelWidth = 0f;
                EditorGUIUtility.fieldWidth = 0f;

                EditorGUI.BeginChangeCheck();
                Vector4 vec = prop.vectorValue;
                EditorGUILayout.LabelField(label);
                vec.x = EditorGUILayout.Slider("x", vec.x, m_Min.x, m_Max.x);
                vec.y = EditorGUILayout.Slider("y", vec.y, m_Min.y, m_Max.y);
                vec.z = EditorGUILayout.Slider("z", vec.z, m_Min.z, m_Max.z);
                vec.w = EditorGUILayout.Slider("w", vec.w, m_Min.w, m_Max.w);

                if (EditorGUI.EndChangeCheck())
                {
                    prop.vectorValue = vec;
                }
            }
            else
            {
                editor.DefaultShaderProperty(prop, label.text);
            }

        }
        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return m_Height;
        }
    }
}
