using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
namespace LcLTools
{
    [CustomEditor(typeof(ShaderStripperAssets))]
    public class ShaderStripperAssetsInspector : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var m_target = target as ShaderStripperAssets;

            VisualElement root = new VisualElement();
            // Label label = new Label("This is a custom inspector for ShaderStripperAssets");

            // create a new property field and bind it to the Active property
            var activeProperty = serializedObject.FindProperty("Active");
            var activeField = new PropertyField(activeProperty);
            activeField.Bind(serializedObject);
            root.Add(activeField);

            // shaderExcludeShaderList
            var shaderExcludeShaderListProperty = serializedObject.FindProperty("shaderExcludeShaderList");
            var shaderExcludeShaderListField = new PropertyField(shaderExcludeShaderListProperty);
            shaderExcludeShaderListField.Bind(serializedObject);
            root.Add(shaderExcludeShaderListField);
            ChangeToggleLabel(shaderExcludeShaderListField, "剔除整个Shader");

            // shaderExcludePassTypeList
            var shaderExcludePassTypeListProperty = serializedObject.FindProperty("shaderExcludePassTypeList");
            var shaderExcludePassTypeListField = new PropertyField(shaderExcludePassTypeListProperty);
            shaderExcludePassTypeListField.Bind(serializedObject);
            root.Add(shaderExcludePassTypeListField);
            ChangeToggleLabel(shaderExcludePassTypeListField, "剔除PassType");


            // create a new property field and bind it to the serialized property
            var variantExcludeProperty = serializedObject.FindProperty("variantExcludeList");
            var variantExcludeField = new PropertyField(variantExcludeProperty);
            variantExcludeField.Bind(serializedObject);
            root.Add(variantExcludeField);

            variantExcludeField.RegisterCallback<GeometryChangedEvent>((EventCallback<GeometryChangedEvent>)(evt =>
            {
                var toggleLabels = variantExcludeField.Query<Label>(className: "unity-toggle__text").ToList();
                foreach (var label in toggleLabels)
                {
                    if (label.text == "Variant Exclude List")
                    {
                        label.text = "剔除包含以下Keyword的变体";
                    }
                    else if (label.text == "Reserved Shader List")
                    {
                        label.text = "保留Shader列表";
                    }
                }


                var addButton = variantExcludeField.Query<Button>(name: "unity-list-view__add-button").Last();
                if (addButton != null)
                {
                    addButton.clickable = new Clickable((() =>
                    {
                        m_target.variantExcludeList.Add(new VariantStripData() { keyword = "_KeyWord" });
                    }));
                }
            }));
            return root;
        }
        // change toggle label text
        private void ChangeToggleLabel(PropertyField field, string text)
        {
            field.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                var toggleLabels = field.Q<Label>(className: "unity-toggle__text");
                if (toggleLabels != null)
                {
                    toggleLabels.text = text;
                }
            });
        }

    }
}