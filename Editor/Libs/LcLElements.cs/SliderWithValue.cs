using System.CodeDom.Compiler;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;

namespace LcLTools
{
    /// <summary>
    /// 有Value显示的Slider
    /// </summary>
    public class SliderWithValue : Slider
    {

        public new class UxmlFactory : UxmlFactory<SliderWithValue, UxmlTraits> { }

        private readonly FloatField _integerElement;

        public override float value
        {
            set
            {
                base.value = value;

                if (_integerElement != null)
                {
                    _integerElement.SetValueWithoutNotify(base.value);
                }
            }
        }

        // ---------------------------------------------------------
        public SliderWithValue() : this(null, 0, 10)
        {

        }

        // ---------------------------------------------------------
        public SliderWithValue(float start, float end, SliderDirection direction = SliderDirection.Horizontal, float pageSize = 0)
            : this(null, start, end, direction, pageSize)
        {
        }

        // ---------------------------------------------------------
        public SliderWithValue(string label, float start = 0, float end = 10, SliderDirection direction = SliderDirection.Horizontal, float pageSize = 0)
            : base(label, start, end, direction, pageSize)
        {

            _integerElement = new FloatField();
            _integerElement.style.flexGrow = 0f;
            _integerElement.RegisterValueChangedCallback(evt =>
            {
                value = evt.newValue;
            });

            Add(_integerElement);

            _integerElement.SetValueWithoutNotify(value);
        }
    }
}
