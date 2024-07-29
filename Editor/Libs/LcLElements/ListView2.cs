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
        private readonly string headerRowName = "ListViewHeaderRow";
        private VisualElement m_HeaderRow;

        VisualElement HeaderRow
        {
            get
            {
                if (m_HeaderRow == null)
                {
                    m_HeaderRow = this.Q<VisualElement>(headerRowName);
                    if (m_HeaderRow == null)
                    {
                        var color = new Color(0, 0, 0, 1);
                        var width = 1;
                        m_HeaderRow = new VisualElement()
                        {
                            name = headerRowName,
                            style =
                            {
                                borderTopColor = color,
                                borderLeftColor = color,
                                borderRightColor = color,
                                borderTopWidth = width,
                                borderLeftWidth = width,
                                borderRightWidth = width,
                            }
                        };
                    }
                }

                return m_HeaderRow;
            }
        }

        public Func<VisualElement> makeHeaderRow { get; set; }


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

        public List<VisualElement> visualElements
        {
            get { return this.Query<VisualElement>(className: "unity-list-view__item").ToList(); }
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

            unbindItem = (element, index) => { m_VisibleElements[index] = null; };

            //Table标题头
            RegisterCallback<GeometryChangedEvent>(evt =>
            {
                if (makeHeaderRow != null && m_HeaderRow == null)
                {
                    this.Q<ScrollView>().parent.Insert(0, HeaderRow);
                    HeaderRow.Add(makeHeaderRow());
                }
            });
            var scrollView = this.Q<ScrollView>();
            scrollView.verticalScroller.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                HeaderRow.style.paddingRight = scrollView.verticalScroller.layout.width;
            });
            scrollView.horizontalScroller.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                HeaderRow.style.paddingBottom = scrollView.horizontalScroller.layout.height;
            });
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
