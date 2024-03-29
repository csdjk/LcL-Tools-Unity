using UnityEngine;
using UnityEditor;
using System;

namespace LcLShaderEditor
{
      /// <summary>
    /// RenderType
    /// </summary>
    public class RenderTypeDrawer : MaterialPropertyDrawer
    {
        override public void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMixedValue;
            var value = (SurfaceType)prop.floatValue;
            var newValue = (SurfaceType)EditorGUI.Popup(position, label, (int)value, Enum.GetNames(typeof(SurfaceType)));
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
            {
                prop.floatValue = (float)newValue;
                var material = editor.target as Material;
                if (material != null)
                {

                    switch (newValue)
                    {
                        case SurfaceType.Opaque:
                            material.SetOverrideTag("RenderType", "Opaque");
                            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
                            material.SetShaderPassEnabled("DepthOnly", false);
                            break;
                        case SurfaceType.Transparent:
                            material.SetOverrideTag("RenderType", "Transparent");
                            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                            material.SetShaderPassEnabled("DepthOnly", true);
                            break;
                    }
                }
            }

        }

    }
}
