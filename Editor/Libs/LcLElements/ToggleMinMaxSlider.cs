using System.CodeDom.Compiler;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;

namespace LcLTools
{
    public class ToggleMinMaxSlider : MinMaxSlider
    {
        public bool Active
        {
            get
            {
                return activeToggle.value;
            }
            set
            {
                activeToggle.value = value;
            }
        }
        private Toggle activeToggle;
        private FloatField minValueField;
        private FloatField maxValueField;
        private VisualElement sliderElement;

        public ToggleMinMaxSlider(string label, Vector2 defaultValue, Vector2 range, bool active = false) : base(label)
        {
            this.value = defaultValue;
            this.lowLimit = range.x;
            this.highLimit = range.y;
            activeToggle = new Toggle() { value = active };
            minValueField = new FloatField() { value = defaultValue.x };
            minValueField.AddToClassList("min-max-slider-input");
            maxValueField = new FloatField() { value = defaultValue.y };
            maxValueField.AddToClassList("min-max-slider-input");

            this.Insert(1, activeToggle);
            this.Insert(2, minValueField);
            this.Add(maxValueField);
            sliderElement = this.ElementAt(3);
            SetActive(active);

            activeToggle.RegisterCallback<ChangeEvent<bool>>((evt) =>
            {
                SetActive(evt.newValue);
            });
            maxValueField.RegisterCallback<ChangeEvent<float>>((evt) =>
            {
                this.maxValue = evt.newValue;
            });
            minValueField.RegisterCallback<ChangeEvent<float>>((evt) =>
            {
                this.minValue = evt.newValue;
            });
            this.RegisterCallback<ChangeEvent<Vector2>>((evt) =>
            {
                minValueField.value = evt.newValue.x;
                maxValueField.value = evt.newValue.y;
            });
        }

        private void SetActive(bool v)
        {
            minValueField.SetEnabled(v);
            sliderElement.SetEnabled(v);
            maxValueField.SetEnabled(v);
        }
    }

}
