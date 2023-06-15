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
    /// RadioButton
    /// </summary>
    public class TableButton : BaseField<int>
    {
        public static StyleSheet styleSheet => LcLEditorUtilities.GetStyleSheet("UIElementsExtend");
        private const string ussFieldInput = "table-container";
        private const string ussUnityButton = "unity-button";
        private const string ussTableButton = "table-button";

        private const string ussTableButtonChild = "table-button-child";

        private int currentIndex = 0;
        // public int defaultIndex = 0;
        private List<RadioButton> list = new List<RadioButton>();

        private int m_DefaultIndex;
        public int defaultIndex
        {
            get { return m_DefaultIndex; }
            set
            {
                m_DefaultIndex = value;
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].SetValueWithoutNotify(i == defaultIndex);
                }
            }
        }

        private Action<int> m_Callback;

        // public TableButton() : this(null, null) { }

        // ---------------------------------------------------------
        public TableButton(string[] labels, int defaultIndex = 0) : base(null, null)
        {
            styleSheets.Add(styleSheet);
            this.AddToClassList(ussFieldInput);
            this.RemoveAt(0);
            this.Init(labels);
            this.defaultIndex = defaultIndex;
        }

        void Init(string[] labels)
        {
            float width = 1.0f / (float)labels.Length;
            for (int i = 0; i < labels.Length; i++)
            {
                var index = i;
                var label = labels[index];
                var radio = new RadioButton() { text = label };
                radio.style.flexGrow = width;
                radio.AddToClassList(ussUnityButton);
                radio.AddToClassList(ussTableButton);
                radio.ElementAt(0).AddToClassList(ussTableButtonChild);
                radio.RegisterValueChangedCallback((ChangeEvent<bool> evt) =>
                {
                    if (evt.newValue)
                    {
                        using (var changeEvent = ChangeEvent<int>.GetPooled(currentIndex, index))
                        {
                            changeEvent.target = this;
                            this.SendEvent(changeEvent);
                        }
                        currentIndex = index;
                        this.SetValueWithoutNotify(currentIndex);
                    }
                });
                this.Add(radio);
                list.Add(radio);
            }
        }
        public override void SetValueWithoutNotify(int index)
        {
            base.SetValueWithoutNotify(index);
        }
    }

}
