using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;
using System.Linq;
// using Unity.Mathematics;
using UnityEditor.UIElements;

public class SetMaterialPropertyWindow : EditorWindow
{
    private TextField m_OnlyTextField;
    private Toggle m_ColorGammaField;
    private Toggle m_ColorLinearField;

    [MenuItem("LcLTools/RenderDoc/批量设置材质属性")]
    private static void ShowWindow()
    {
        var window = GetWindow<SetMaterialPropertyWindow>();
        window.titleContent = new GUIContent("批量设置材质属性");
        window.Show();
    }


    void OnEnable()
    {
    }

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;

        var label = new Label("Hello World!");
        root.Add(label);

        var materialField = new ObjectField("Material")
        {
            objectType = typeof(UnityEngine.Object), // 允许拖入 GameObject 或 Material
            allowSceneObjects = true // 允许拖入场景中的对象
        };
        materialField.RegisterValueChangedCallback(evt =>
        {
            if (evt.newValue is GameObject go)
            {
                var renderer = go.GetComponent<Renderer>();
                if (renderer != null)
                {
                    materialField.value = renderer.sharedMaterial;
                }
            }
        });
        root.Add(materialField);

        m_OnlyTextField = new TextField("只设置包含该字符串的属性");
        root.Add(m_OnlyTextField);

        m_ColorGammaField = new Toggle("Color To Gamma")
        {
            value = true
        };
        root.Add(m_ColorGammaField);

        m_ColorLinearField = new Toggle("Color To Linear");
        root.Add(m_ColorLinearField);

        var textField = new TextField("Property String")
        {
            multiline = true,
            value = ""
        };

        var scrollView = new ScrollView();
        scrollView.Add(textField);
        root.Add(scrollView);

        var button = new Button(() => { SetMaterialProperty(materialField.value as Material, textField.value); })
        {
            text = "Set Material Property",
            style =
            {
                position = Position.Absolute,
                bottom = 10,
                left = 0,
                right = 0,
                height = 30
            }
        };
        root.Add(button);
    }

    private void SetMaterialProperty(Material materialFieldValue, string textFieldValue)
    {
        if (materialFieldValue == null)
        {
            Debug.LogError("Material is null");
            return;
        }

        var properties = ParseProperties(textFieldValue);
        foreach (var property in properties)
        {
            var lowName = property.Value.name.ToLower();
            if (!lowName.Contains(m_OnlyTextField.value.ToLower()) )
            {
                continue;
            }


            if (property.Value.type == "float4" || property.Value.type == "float3")
            {
                Vector4 propertyVector = new Vector4(property.Value.floatValues[0], property.Value.floatValues[1],
                    property.Value.floatValues[2], 1);
                if (property.Value.type == "float4")
                {
                     propertyVector.w = property.Value.floatValues[3];
                }

                // If the property name contains "color", convert the color to Gamma space
                if (lowName.Contains("color"))
                {
                    Color colorValue = new Color(propertyVector.x, propertyVector.y, propertyVector.z, propertyVector.w);
                    if (m_ColorGammaField.value)
                    {
                        colorValue = colorValue.gamma;
                    }
                    else if (m_ColorLinearField.value)
                    {
                        colorValue = colorValue.linear;
                    }
                    propertyVector = new Vector4(colorValue.r, colorValue.g, colorValue.b, colorValue.a);
                }

                Debug.Log($"Set {property.Value.name} : {propertyVector}");
                materialFieldValue.SetVector(property.Value.name, propertyVector);
            }
            else if (property.Value.type == "float")
            {
                materialFieldValue.SetFloat(property.Value.name, property.Value.floatValues[0]);
                Debug.Log($"Set {property.Value.name} : {string.Join(',', property.Value.floatValues)}");
            }

        }
    }

    public struct Property
    {
        public string name;
        public float[] floatValues;
        public string type;
    }

    public static Dictionary<string, Property> ParseProperties(string propertiesText)
    {
        var properties = new Dictionary<string, Property>();

        var lines = propertiesText.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3) continue;
            var propertyName = parts[0];

            // if (propertyName.Contains("UnusedX") || propertyName.Contains("hlslcc")) continue;
            propertyName = propertyName.Replace("Xhlslcc_UnusedX", "");

            var propertyType = parts[^1].TrimEnd('\r');
            string propertyValue;
            if (parts.Length > 3 && int.TryParse(parts[^2], out _))
            {
                // 如果 parts 的长度大于 3，并且倒数第二个值是 int 类型，那么跳过倒数第二个值
                propertyValue = string.Join(" ", parts.Skip(1).Take(parts.Length - 3));
            }
            else
            {
                // 否则，只取除了第一个和最后一个值之外的值
                propertyValue = string.Join(" ", parts.Skip(1).Take(parts.Length - 2));
            }

            float[] propertyFloatValues = propertyValue.Split(',').Select(float.Parse).ToArray();

            var property = new Property
            {
                name = propertyName,
                floatValues = propertyFloatValues,
                type = propertyType
            };

            properties[propertyName] = property;
        }

        return properties;
    }
}
