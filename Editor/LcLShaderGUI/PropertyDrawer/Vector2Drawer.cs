using UnityEngine;
using UnityEditor;

namespace CustomShaderEditor
{
    public class Vector2Drawer : MaterialPropertyDrawer
    {
        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            if (prop.type == MaterialProperty.PropType.Vector)
            {
                Vector4 vectorValue = prop.vectorValue;
                Vector2 vector2Value = new Vector2(vectorValue.x, vectorValue.y);

                EditorGUI.BeginChangeCheck();
                {
                    vector2Value = EditorGUI.Vector2Field(position, label, vector2Value);
                }
                if (EditorGUI.EndChangeCheck())
                {
                    prop.vectorValue = new Vector4(vector2Value.x, vector2Value.y, vectorValue.z, vectorValue.w);
                }
            }
            else
            {
                editor.DefaultShaderProperty(prop, label.text);
            }
        }

        // public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        // {
        //     return (prop.type == MaterialProperty.PropType.Vector) ? base.GetPropertyHeight(prop, label, editor) : 0;
        // }
    }
}
