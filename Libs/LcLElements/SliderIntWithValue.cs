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
    /// 有Value显示的IntSlider 
    /// </summary>
    public class SliderIntWithValue : SliderInt
    {
        public new class UxmlFactory : UxmlFactory<SliderIntWithValue, UxmlTraits> { }

        private readonly IntegerField _integerElement;

        public override int value
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
        public SliderIntWithValue() : this(null, 0, 10)
        {

        }

        // ---------------------------------------------------------
        public SliderIntWithValue(int start, int end, SliderDirection direction = SliderDirection.Horizontal, int pageSize = 0)
            : this(null, start, end, direction, pageSize)
        {
        }

        // ---------------------------------------------------------
        public SliderIntWithValue(string label, int start = 0, int end = 10, SliderDirection direction = SliderDirection.Horizontal, float pageSize = 0)
            : base(label, start, end, direction, pageSize)
        {

            _integerElement = new IntegerField();
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
