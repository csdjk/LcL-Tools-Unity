using UnityEngine;
using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace LcLTools
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ButtonAttribute : PropertyAttribute
    {
        public string name;
        public Color enableColor = Color.green;
        public Color disableColor = Color.white;
        public ButtonAttribute()
        {
        }
        public ButtonAttribute(string name)
        {
            this.name = name;
        }
        public ButtonAttribute(string name, string itemName)
        {
            this.name = name;
        }

        public ButtonAttribute(string name, Color activeColor, Color disableColor)
        {
            this.name = name;
            this.enableColor = activeColor;
            this.disableColor = disableColor;
        }

    }

    [CustomPropertyDrawer(typeof(ButtonAttribute))]
    public class ButtonDrawer : PropertyDrawer
    {
        readonly Color multiplier = new Color(0.345f, 0.345f, 0.345f, 1);

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            ButtonAttribute attr = (ButtonAttribute)attribute;
            var button = new Button()
            {
                text = attr.name,
                style = { backgroundColor = attr.enableColor * multiplier }
            };

            // 转换Gamma颜色空间
            // 取消button的focus
            button.focusable = false;
            button.clickable = new Clickable((EventBase evt) =>
            {
                property.boolValue = !property.boolValue;
                button.style.backgroundColor = (property.boolValue ? attr.enableColor : attr.disableColor) * multiplier;
                property.serializedObject.ApplyModifiedProperties();
            });

            return button;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 绘制button
            ButtonAttribute attr = (ButtonAttribute)attribute;
            Color defaultColor = EditorStyles.label.normal.textColor;
            var color = GUI.backgroundColor;

            GUI.backgroundColor = property.boolValue ? attr.enableColor : attr.disableColor;

            if (GUI.Button(position, new GUIContent(attr.name)))
            {
                property.boolValue = !property.boolValue;
                property.serializedObject.ApplyModifiedProperties();
            }

            GUI.backgroundColor = color;
        }

    }
}
