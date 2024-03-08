using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine.Rendering;
using LcLTools;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System;
using static UnityEngine.ShaderVariantCollection;
namespace LcLTools
{
    public class ShaderVariantAddWindow : EditorWindow
    {

        private Shader m_Shader;
        private PassType m_PassType;
        private ShaderVariantCollectionMapper m_Mapper;

        List<string> m_AvailableKeywords = new List<string>();
        private List<string> m_SelectedKeywords = new List<string>();
        private int m_SelectedShaderKeywordIndex = 0;
        GridView m_AvailableKeywordsGridView;
        GridView m_SelectedShaderKeywordsGridView;
        ListView m_VariantListView;
        Button m_AddVariantButton;
        Label m_MessageLabel;
        int m_MaxVisibleVariants = 256;
        List<int> m_FilteredVariantTypes = new List<int>();
        List<string[]> m_FilteredVariantKeywords = new List<string[]>();
        List<int> m_SelectedVariants = new List<int>();

        // static ShaderVariantAddWindow Instance = null;
        public static void ShowWindow(Shader shader, PassType passType, ShaderVariantCollectionMapper mapper)
        {
            // if (Instance)
            // {
            //     Instance.Close();
            // }
            var window = GetWindow<ShaderVariantAddWindow>();
            window.titleContent = new GUIContent("ShaderVariantAddWindow");
            window.Init(shader, passType, mapper);

            window.Show();
        }

        public void Init(Shader shader, PassType passType, ShaderVariantCollectionMapper mapper)
        {
            m_Shader = shader;
            m_PassType = passType;
            m_Mapper = mapper;
            m_SelectedKeywords.Clear();
            ApplyKeywordFilter();
            InitUI();
        }
        void ApplyKeywordFilter()
        {
            var data = ShaderUtilImpl.GetShaderVariantEntriesFilteredInternal(m_Shader, m_MaxVisibleVariants, m_SelectedKeywords.ToArray(), m_Mapper.collection);
            m_SelectedVariants.Clear();
            m_FilteredVariantKeywords.Clear();
            m_FilteredVariantTypes.Clear();
            m_AvailableKeywords.Clear();
            for (var i = 0; i < data.passTypes.Length; ++i)
            {
                var passType = data.passTypes[i];
                if (passType == (int)m_PassType)
                {
                    m_FilteredVariantTypes.Add(passType);
                    m_FilteredVariantKeywords.Add(data.keywordLists[i].Split(' '));
                }
            }
            m_AvailableKeywords.InsertRange(0, data.remainingKeywords);
            m_AvailableKeywords.Sort();

            m_SelectedShaderKeywordIndex = 0;
        }

        void Refresh()
        {
            ApplyKeywordFilter();
            m_SelectedShaderKeywordsGridView.Rebuild();
            m_AvailableKeywordsGridView.Rebuild();
            m_VariantListView.Rebuild();
            UpdateSelectedVariantsLabel();
        }

        void UpdateSelectedVariantsLabel()
        {
            m_AddVariantButton.text = $"添加变体({m_SelectedVariants.Count})";
        }

        public void InitUI()
        {
            var root = this.rootVisualElement;
            root.Clear();
            root.style.marginLeft = 2;
            root.style.marginRight = 2;
            root.style.marginTop = 2;
            root.style.marginBottom = 2;
            // Current shader label
            var shaderLabel = new Label(m_Shader.name)
            {
                style = {
                alignSelf = Align.Center,
                color   = Color.green,
                fontSize = 15
            }
            };
            root.Add(shaderLabel);

            // Pass type dropdown
            var passTypeEnumField = new EnumField("PassType", m_PassType)
            {
                style = {
                marginBottom = 5,
                marginTop = 5,
            }
            };
            passTypeEnumField.RegisterValueChangedCallback(evt =>
            {
                m_PassType = (PassType)evt.newValue;
                // Refresh();
                Init(m_Shader, m_PassType, m_Mapper);
            });
            root.Add(passTypeEnumField);


            // create TwoPaneSplitView container
            var container = new TwoPaneSplitView(0, 150, TwoPaneSplitViewOrientation.Vertical);
            root.Add(container);


            m_AvailableKeywordsGridView = new GridView(m_AvailableKeywords, 20, (i) =>
            {
                var lable = new Label();
                lable.AddToClassList("unity-button");

                var keyword = m_AvailableKeywords[i];
                lable.text = keyword;
                lable.AddManipulator(new Clickable(() =>
                {
                    if (!m_SelectedKeywords.Contains(keyword))
                    {
                        m_SelectedKeywords.Add(keyword);
                        m_SelectedKeywords.Sort();
                        m_AvailableKeywords.Remove(keyword);
                        Refresh();
                    }
                }));
                return lable;
            });
            m_AvailableKeywordsGridView.showFoldoutHeader = true;
            m_AvailableKeywordsGridView.headerTitle = "待添加Keyword：";
            container.Add(m_AvailableKeywordsGridView);


            var container2 = new TwoPaneSplitView(0, 100, TwoPaneSplitViewOrientation.Vertical);
            container.Add(container2);


            m_SelectedShaderKeywordsGridView = new GridView(m_SelectedKeywords, 20, (i) =>
            {
                var lable = new Label();
                lable.AddToClassList("unity-button");

                var keyword = m_SelectedKeywords[i];
                lable.text = keyword;
                lable.AddManipulator(new Clickable(() =>
                {
                    m_AvailableKeywords.Add(keyword);
                    m_SelectedKeywords.Remove(keyword);
                    Refresh();
                }));
                return lable;
            });
            m_SelectedShaderKeywordsGridView.showFoldoutHeader = true;
            m_SelectedShaderKeywordsGridView.headerTitle = "当前Keyword：";
            container2.Add(m_SelectedShaderKeywordsGridView);


            m_VariantListView = new ListView(m_FilteredVariantKeywords, 20, () =>
            {
                var toggle = new Toggle();
                var toggleInput = toggle.Q<VisualElement>(className: "unity-toggle__input");
                toggleInput.style.flexGrow = 0;

                var box = new VisualElement() { name = "keyword" };
                box.style.flexDirection = FlexDirection.Row;
                box.style.alignItems = Align.Center;
                toggle.Add(box);
                toggle.RegisterValueChangedCallback((e) =>
                {
                    var toggle = e.target as Toggle;
                    int i = (int)toggle.userData;
                    if (e.newValue)
                    {
                        if (!m_SelectedVariants.Contains(i))
                            m_SelectedVariants.Add(i);
                    }
                    else
                    {
                        if (m_SelectedVariants.Contains(i))
                            m_SelectedVariants.Remove(i);
                    }
                    UpdateSelectedVariantsLabel();
                });
                return toggle;
            }, (e, i) =>
            {
                var toggle = e as Toggle;
                toggle.userData = i;
                toggle.value = m_SelectedVariants.Contains(i);

                var box = toggle.Q<VisualElement>(name: "keyword");
                box?.Clear();

                var keywords = m_FilteredVariantKeywords[i];
                if (string.IsNullOrEmpty(keywords[0]))
                {
                    toggle.text = "<No Keywords>";
                    return;
                }

                foreach (var keyword in keywords)
                {
                    var lable = new Label();
                    lable.text = keyword;
                    if (m_SelectedKeywords.Contains(keyword))
                        lable.style.color = new StyleColor() { value = Color.green };
                    lable.style.marginLeft = 5;
                    box.Add(lable);
                }
            });
            m_VariantListView.headerTitle = "变体列表：";
            m_VariantListView.showFoldoutHeader = true;
            m_VariantListView.showBorder = true;
            m_VariantListView.horizontalScrollingEnabled = true;
            m_VariantListView.selectionType = SelectionType.Single;
            m_VariantListView.onSelectionChange += (e) =>
            {
                Debug.Log("onSelectionChange");
            };
            container2.Add(m_VariantListView);

            // Add variant button
            m_AddVariantButton = new Button(() => AddVariant()) { text = "添加变体" };
            root.Add(m_AddVariantButton);

            // Message label
            m_MessageLabel = new Label("")
            {
                style = {
                alignSelf = Align.Center,
                whiteSpace = WhiteSpace.Normal,
                marginBottom = 10,
                marginTop = 10,
            }
            };
            root.Add(m_MessageLabel);
        }

        private void AddVariant()
        {
            Undo.RecordObject(m_Mapper.collection, "Add variant");

            string message = "";

            for (var i = 0; i < m_SelectedVariants.Count; ++i)
            {
                try
                {
                    var index = m_SelectedVariants[i];
                    var variant = new ShaderVariant(m_Shader, m_PassType, m_FilteredVariantKeywords[index]);
                    string keywordString = string.Join(", ", m_FilteredVariantKeywords[index]);

                    if (m_Mapper.HasVariant(variant))
                    {
                        message += $"变体<{m_PassType}>[{keywordString}]已存在";
                    }
                    else
                    {
                        message += $"添加变体<{m_PassType}>[{keywordString}]";
                        m_Mapper.AddVariant(variant);
                    }
                    if (i != m_SelectedVariants.Count - 1) message += "\n";
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }
            m_MessageLabel.text = message;
            m_MessageLabel.style.color = new StyleColor() { value = Color.green };
            Refresh();
            ShaderCollectionWindow.Instance.RefreshShaderList();

        }
    }
}
