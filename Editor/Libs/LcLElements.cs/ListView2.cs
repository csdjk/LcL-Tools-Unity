using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LcLTools
{
    public class ListView2 : ListView
    {
        private List<VisualElement> m_VisibleElements = new();

        public IEnumerable<VisualElement> selectedVisualElements
        {
            get
            {
                List<VisualElement> selectedElements = new List<VisualElement>();
                foreach (var index in this.selectedIndices)
                {
                    if (m_VisibleElements[index] != null)
                    {
                        selectedElements.Add(m_VisibleElements[index]);
                    }
                }

                return selectedElements;
            }
        }

        public ListView2(IList itemsSource, int itemHeight, Func<VisualElement> makeItem, Action<VisualElement, int> bindItem)
            : base(itemsSource, itemHeight, makeItem, bindItem)
        {
            this.bindItem = (element, index) =>
            {
                if (m_VisibleElements.Count <= index)
                {
                    m_VisibleElements.Add(element);
                }
                else
                {
                    m_VisibleElements[index] = element;
                }

                bindItem(element, index);
            };

            this.unbindItem = (element, index) => { m_VisibleElements[index] = null; };
        }

        public VisualElement GetVisualElementAt(int index)
        {
            if (m_VisibleElements.Count <= index)
            {
                return null;
            }

            return m_VisibleElements[index];
        }

        public void RemoveSelectedElement()
        {
            foreach (var index in this.selectedIndices)
            {
                if (itemsSource.Count > index)
                {
                    itemsSource.RemoveAt(index);
                }
            }

            this.Rebuild();
        }

        public void RemoveElement(object element)
        {
            itemsSource.Remove(element);
            this.Rebuild();
        }
    }
}