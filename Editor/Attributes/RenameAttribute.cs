using System.Diagnostics;
using UnityEngine;
using System;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Text.RegularExpressions;

namespace LcLTools
{
    /// <summary>
    /// 重命名属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class RenameAttribute : PropertyAttribute
    {
        public string name = "";
        public string itemName;
        public Color nameColor = Color.white;
        public RenameAttribute(string name)
        {
            this.name = name;
        }
        public RenameAttribute(string name, string itemName)
        {
            this.name = name;
            this.itemName = itemName;
        }

        public RenameAttribute(string name, Color nameColor)
        {
            this.name = name;
            this.nameColor = nameColor;
        }

    }

    [CustomPropertyDrawer(typeof(RenameAttribute))]
    public class RenameDrawer : PropertyDrawer
    {
        // 判断是否Array或者List
        private bool IsArrayOrList(SerializedProperty property)
        {
            return property.propertyPath.Contains(".Array.") || property.propertyPath.Contains("List");
        }
        // 获取名字
        private string GetName(SerializedProperty property)
        {
            RenameAttribute rename = (RenameAttribute)attribute;
            string name = property.displayName;
            if (IsArrayOrList(property))
            {
                if (rename.itemName != null)
                {
                    // 从property.propertyPath 中提取出数组的index
                    // 例如assetsIncludeFolderList.Array.data[1]
                    string pattern = @"\[(\d+)\]";
                    Match match = Regex.Match(property.propertyPath, pattern);
                    if (match.Success)
                    {
                        string index = match.Groups[1].Value;
                        name = rename.itemName.Equals(String.Empty) ? index : rename.itemName + " " + index;
                    }
                }

            }
            else
            {
                name = rename.name;
            }
            return name;
        }
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // 替换属性名称
            RenameAttribute rename = (RenameAttribute)attribute;
            var propertyField = new PropertyField(property);

            propertyField.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                var label = propertyField.Q<Label>(className: "unity-toggle__text");
                if (label != null)
                {
                    label.text = GetName(property);
                }
            });
            return propertyField;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 替换属性名称
            RenameAttribute rename = (RenameAttribute)attribute;
            label.text = GetName(property);
            // 重绘GUI
            Color defaultColor = EditorStyles.label.normal.textColor;
            EditorStyles.label.normal.textColor = rename.nameColor;
            EditorGUI.PropertyField(position, property, label, true);
            EditorStyles.label.normal.textColor = defaultColor;
        }

    }
}
