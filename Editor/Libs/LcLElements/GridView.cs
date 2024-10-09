
using System.CodeDom.Compiler;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections;
namespace LcLTools
{
    public class GridView : BindableElement
    {
        public static StyleSheet styleSheet => LcLEditorUtilities.GetStyleSheet("UIElementsExtend");

        public static readonly string ussClassName = "grid-";
        private static readonly string ussContainer = ussClassName + "container";
        private static readonly string ussGridItem = ussClassName + "item";
        private static readonly string ussGridScrollViewContentContainer = ussClassName + "scrollview-content-container";
        private static readonly string foldoutHeaderUssClassName = ussClassName + "foldout-content";
        public static readonly string arraySizeFieldUssClassName = ussClassName + "size-field";
        public static readonly string emptyLabelUssClassName = ussClassName + "empty-label";


        string m_HeaderTitle;
        public string headerTitle
        {
            get => m_HeaderTitle;
            set
            {
                m_HeaderTitle = value;
                if (m_Foldout != null)
                    m_Foldout.text = m_HeaderTitle;
            }
        }

        Foldout m_Foldout;
        TextField m_ArraySizeField;
        private bool m_ShowFoldoutHeader;
        public bool showFoldoutHeader
        {
            get => m_ShowFoldoutHeader;
            set
            {
                if (m_ShowFoldoutHeader == value)
                    return;

                m_ShowFoldoutHeader = value;

                // EnableInClassList(listViewWithHeaderUssClassName, value);

                if (m_ShowFoldoutHeader)
                {
                    if (m_Foldout != null)
                        return;

                    m_Foldout = new Foldout() { name = foldoutHeaderUssClassName, text = m_HeaderTitle };
                    m_Foldout.AddToClassList(foldoutHeaderUssClassName);
                    m_Foldout.tabIndex = 1;
                    hierarchy.Add(m_Foldout);
                    m_Foldout.Add(scrollView);
                    var unityFoldoutContent = m_Foldout.Q<VisualElement>(className: "unity-foldout__content");
                    unityFoldoutContent.style.height = Length.Percent(100);
                    unityFoldoutContent.style.marginLeft = 0;

                }
                else if (m_Foldout != null)
                {
                    m_Foldout?.RemoveFromHierarchy();
                    m_Foldout = null;
                    hierarchy.Add(scrollView);
                }

                SetupArraySizeField();
                UpdateEmpty();
                // if (showAddRemoveFooter)
                // {
                //     EnableFooter(true);
                // }
            }
        }

        void SetupArraySizeField()
        {
            if (!showFoldoutHeader)
            {
                m_ArraySizeField?.RemoveFromHierarchy();
                m_ArraySizeField = null;
                return;
            }

            m_ArraySizeField = new TextField() { name = arraySizeFieldUssClassName };
            m_ArraySizeField.AddToClassList(arraySizeFieldUssClassName);
            m_ArraySizeField.isDelayed = true;
            m_ArraySizeField.focusable = true;
            hierarchy.Add(m_ArraySizeField);

            UpdateArraySizeField();
        }
        bool HasValidDataAndBindings()
        {
            return itemsSource != null && makeItem != null;
        }
        void UpdateArraySizeField()
        {
            if (!HasValidDataAndBindings())
                return;

            m_ArraySizeField?.SetValueWithoutNotify(m_Items.Count.ToString());
        }

        Label m_EmptyListLabel;
        void UpdateEmpty()
        {
            if (!HasValidDataAndBindings())
                return;

            if (itemsSource.Count == 0)
            {
                if (m_EmptyListLabel != null)
                    return;

                m_EmptyListLabel = new Label("List is Empty");
                m_EmptyListLabel.AddToClassList(emptyLabelUssClassName);
                scrollView.contentViewport.Add(m_EmptyListLabel);
            }
            else
            {
                m_EmptyListLabel?.RemoveFromHierarchy();
                m_EmptyListLabel = null;
            }
        }

        ScrollView scrollView => m_ScrollView;
        private ScrollView m_ScrollView;
        Func<int, VisualElement> m_MakeItem;
        private IList itemsSource;
        private List<VisualElement> m_Items = new List<VisualElement>();

        public Func<int, VisualElement> makeItem
        {
            get => m_MakeItem;
            set
            {
                m_MakeItem = value;
                // Rebuild();
            }
        }


        public event Action<object, int> onSelectionChange;
        object selectedItem => itemsSource[m_SelectedIndex];
        private int m_SelectedIndex = -1;
        public int selectedIndex
        {
            get => m_SelectedIndex;
            set { m_SelectedIndex = value; }
        }

        // ---------------------------------------------------------
        public GridView(IList itemsSource, float itemHeight = 20, Func<int, VisualElement> makeItem = null)
        {
            this.itemsSource = itemsSource;
            this.makeItem = makeItem;
            styleSheets.Add(styleSheet);
            this.AddToClassList(ussContainer);
            this.Init();
        }

        void Init()
        {
            m_ScrollView = new ScrollView();
            m_ScrollView.AddToClassList("grid-scrollview");
            var unityContentContainer = m_ScrollView.Q("unity-content-container");
            unityContentContainer.AddToClassList(ussGridScrollViewContentContainer);
            Add(m_ScrollView);
            CreateItems();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (evt.pointerType == UnityEngine.UIElements.PointerType.mouse)
            {
                var target = evt.target as VisualElement;
                selectedIndex = target.parent.IndexOf(target);
                onSelectionChange?.Invoke(selectedItem, selectedIndex);
            }
        }

        // rebuild the list
        public void Rebuild()
        {
            if (m_Items.Count > 0)
            {
                foreach (var item in m_Items)
                {
                    item.RemoveFromHierarchy();
                }
                m_Items.Clear();
            }
            CreateItems();
            SetupArraySizeField();
            UpdateEmpty();
        }

        void CreateItems()
        {
            for (int i = 0; i < itemsSource.Count; i++)
            {
                var item = makeItem(i);
                item.focusable = true;
                item.AddToClassList(ussGridItem);
                item.RegisterCallback<PointerUpEvent>(OnPointerUp);
                m_Items.Add(item);
                m_ScrollView.Add(item);
            }
        }

    }

}
