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
        float height;

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            height = position.height;
            editor.TexturePropertySingleLine(label, prop);
        }
        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return height;
        }
    }

    /// <summary>
    /// 文件夹
    /// </summary>
    public class FoldoutDrawer : MaterialPropertyDrawer
    {
        string keyword;
        string foldoutValueName;
        Material mat;
        bool isKeyword => keyword != null;

        public FoldoutDrawer()
        {
        }

        public FoldoutDrawer(string keyword)
        {
            this.keyword = keyword;
            foldoutValueName = ShaderEditorHandler.GetFoldoutPropName(keyword);
        }

        void SetKeyword(Material material, bool on)
        {
            if (on)
                material.EnableKeyword(keyword);
            else
                material.DisableKeyword(keyword);
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            mat = prop.targets[0] as Material;
            if (foldoutValueName == null)
            {
                foldoutValueName = ShaderEditorHandler.GetFoldoutPropName(prop.name);
            }

            var serializedObject = new SerializedObject(mat);

            var foldoutValue = serializedObject.GetHiddenPropertyFloat(foldoutValueName);
            var foldout = foldoutValue > 0;
            var toggleValue = prop.floatValue > 0;
            foldout = ShaderEditorHandler.Foldout(position, foldout, label.text, isKeyword, ref toggleValue);

            prop.floatValue = Convert.ToSingle(toggleValue);
            serializedObject.SetHiddenPropertyFloat(foldoutValueName, Convert.ToSingle(foldout));

            if (isKeyword)
            {
                SetKeyword(mat, toggleValue);
            }
            serializedObject.Dispose();
        }
        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return 0;
        }

    }

    /// <summary>
    /// Foldout End
    /// </summary>
    public class FoldoutEndDrawer : MaterialPropertyDrawer
    {
        bool condition;
        float height;
        public FoldoutEndDrawer(string foldout)
        {
        }
        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            height = position.height;
            editor.DefaultShaderProperty(prop, label.text);
        }
        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return condition ? height : -2;
        }
    }


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
                            material.SetInt("_ZWrite", 1);
                            material.DisableKeyword("_ALPHA_ON");
                            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
                            material.SetShaderPassEnabled("DepthOnly", false);
                            break;
                        case SurfaceType.Transparent:
                            material.SetOverrideTag("RenderType", "Transparent");
                            material.SetInt("_ZWrite", 0);
                            material.EnableKeyword("_ALPHA_ON");
                            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                            material.SetShaderPassEnabled("DepthOnly", true);
                            break;
                    }
                }
            }

        }

    }


    public class VectorRangeDrawer : MaterialPropertyDrawer
    {
        float height;

        private Vector4 min;
        private Vector4 max;
        public VectorRangeDrawer(float min, float max)
        {
            this.min.Set(min, min, min, min);
            this.max.Set(max, max, max, max);
        }
        public VectorRangeDrawer(float min0, float max0, float min1, float max1, float min2, float max2, float min3, float max3)
        {
            this.min.Set(min0, min1, min2, min3);
            this.max.Set(max0, max1, max2, max3);
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            height = position.height;
            if (prop.type == MaterialProperty.PropType.Vector)
            {
                EditorGUIUtility.labelWidth = 0f;
                EditorGUIUtility.fieldWidth = 0f;

                EditorGUI.BeginChangeCheck();
                Vector4 vec = prop.vectorValue;
                EditorGUILayout.LabelField(label);
                vec.x = EditorGUILayout.Slider("x", vec.x, min.x, max.x);
                vec.y = EditorGUILayout.Slider("y", vec.y, min.y, max.y);
                vec.z = EditorGUILayout.Slider("z", vec.z, min.z, max.z);
                vec.w = EditorGUILayout.Slider("w", vec.w, min.w, max.w);

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
            return height;
        }
    }
}