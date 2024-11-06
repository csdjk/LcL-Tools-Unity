using UnityEngine;
using UnityEditor;

namespace CustomShaderEditor
{
    public class Vector2Drawer : MaterialPropertyDrawer
    {
        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            if (prop.type == MaterialProperty.PropType.Vector)
            {
                Vector4 vectorValue = prop.vectorValue;
                Vector2 vector2Value = new Vector2(vectorValue.x, vectorValue.y);

                EditorGUI.BeginChangeCheck();
                vector2Value = EditorGUI.Vector2Field(position, label, vector2Value);
                // vector2Value = EditorGUILayout.Vector2Field(label, vector2Value);
                if (EditorGUI.EndChangeCheck())
                {
                    prop.vectorValue = new Vector4(vector2Value.x, vector2Value.y, vectorValue.z, vectorValue.w);
                }
            }
            else
            {
                editor.DefaultShaderProperty(prop, label);
                // EditorGUI.LabelField(position, label, "Use Vector2Drawer with Vector properties.");
            }
        }
    }
}
