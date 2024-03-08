
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System.IO;
using UnityEditor.UIElements;
using System.Linq;
using System.Reflection;
using System;
using UnityEngine.Rendering;
using static UnityEngine.ShaderVariantCollection;
using UnityEditor.Experimental.GraphView;

namespace LcLTools
{

    public class ShaderCollectionWindow : EditorWindow
    {
        public static StyleSheet stylesResource => LcLEditorUtilities.GetStyleSheet("ShaderCollection");
        private const string titleClass = "title";
        private const string shaderVariantClass = "shader-variant";
        private const string containerClass = "container";
        private const string leftContainerClass = "left-container";
        private const string centerLineClass = "center-line";
        private const string rightContainerClass = "right-container";
        private const string shaderCollectionAssetsClass = "shader-collection-assets";

        private const string shaderContainerClass = "shader-container";
        private const string shaderContainerTitleClass = "shader-container-title";
        private const string sortBoxClass = "sort-box";
        private const string sortButtonClass = "sort-button";
        private const string shaderItemBoxClass = "shader-item-box";
        // delete button class
        private const string deleteButtonClass = "delete-button";
        private const string variantCountClass = "variant-count";

        private const string shaderVariantContainerClass = "shader-variant-container";
        private const string shaderVariantListViewClass = "shader-variant-list-view";
        private const string shaderCollectButtonClass = "shader-collect-button";
        // shader item box

        //
        private const string shaderVariantCollectionName = "AllShaders.shadervariants";
        private const string defaultShaderVariantCollectionPath = "Assets/Resources/Shaders/" + shaderVariantCollectionName;
        private ObjectField m_ShaderVariantCollectionField;
        private VisualElement m_LeftContainer;
        private TwoPaneSplitView m_RightContainer;
        private ObjectField m_SVCField;
        private VisualElement m_ConfigContainerBox;
        private VisualElement m_ShaderContainer;
        private ListView m_ShaderList;
        private ObjectField m_NewShaderField;
        private TwoPaneSplitView m_MainContainer;

        public Shader CurrentSelectedShader
        {
            get
            {
                if (m_ShaderList.selectedItem == null)
                {
                    return null;
                }
                return m_ShaderList.selectedItem as Shader;
            }
        }

        [SerializeField]
        private ShaderVariantCollectionMapper m_CollectionMapper;
        private VisualElement m_VariantContainer;

        private ShaderVariantCollectionMapper collectionMapper
        {
            get
            {
                if (m_CollectionMapper == null)
                {
                    m_CollectionMapper = new ShaderVariantCollectionMapper(collection);
                }
                return m_CollectionMapper;
            }
        }

        public ShaderVariantCollection collection => m_SVCField?.value as ShaderVariantCollection;

        int m_OldSelectedShaderIndex = 0;
        int m_SelectedShaderIndex
        {
            get => m_ShaderList?.selectedIndex ?? m_OldSelectedShaderIndex;
            // set => m_ShaderList?.SetSelection(value);
        }

        // Instance of the window
        public static ShaderCollectionWindow Instance;


        [MenuItem("LcLTools/Shader变体收集")]
        private static void ShowWindow()
        {
            Instance = GetWindow<ShaderCollectionWindow>();
            Instance.titleContent = new GUIContent("ShaderCollectionWindow");
            Instance.Show();
        }

        private void OnEnable()
        {
            Instance = this;
            m_CollectionMapper = null;
            VisualElement root = rootVisualElement;
            root.styleSheets.Add(stylesResource);

            var title = new Label("Shader变体收集");
            title.AddToClassList(titleClass);
            root.Add(title);

            m_SVCField = new ObjectField("ShaderVariantCollection:");
            m_SVCField.AddToClassList(shaderVariantClass);
            m_SVCField.objectType = typeof(ShaderVariantCollection);
            m_SVCField.value = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(defaultShaderVariantCollectionPath);
            m_SVCField.style.marginLeft = 20;
            m_SVCField.style.marginRight = 20;
            // register a callback on the field when the value changes
            m_SVCField.RegisterValueChangedCallback(evt =>
            {
                RefreshShaderList();
            });

            var createShaderVariantAsset = new Button(() =>
            {
                var shaderVariantCollection = new ShaderVariantCollection();
                var path = AssetDatabase.GenerateUniqueAssetPath(defaultShaderVariantCollectionPath);
                AssetDatabase.CreateAsset(shaderVariantCollection, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                m_SVCField.value = shaderVariantCollection;
            })
            {
                text = "New"
            };
            m_SVCField.Add(createShaderVariantAsset);
            root.Add(m_SVCField);


            // 创建一个左右布局的容器
            m_MainContainer = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);
            root.Add(m_MainContainer);


            // 左边的配置列表
            DrawLeftContainer();

            // 右边的shader列表
            DrawRightContainer();

            var collect = new Button(() =>
            {
                string shaderVariantCollectionPath = m_SVCField.value ? AssetDatabase.GetAssetPath(m_SVCField.value) : defaultShaderVariantCollectionPath;
                ShaderCollection.ALL_SHADER_VARAINT_ASSET_PATH = shaderVariantCollectionPath;
                Debug.Log("ShaderVariantCollectionPath:" + ShaderCollection.ALL_SHADER_VARAINT_ASSET_PATH);
                ShaderCollection.shaderCollectionConfigAssets = m_ShaderVariantCollectionField.value as ShaderCollectionAssets;
                ShaderCollection.CollectShaderVariant();
                m_SVCField.value = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(defaultShaderVariantCollectionPath);
            })
            {
                text = "Shader变体收集"
            };
            collect.AddToClassList(shaderCollectButtonClass);
            root.Add(collect);

            m_ShaderList?.SetSelection(m_SelectedShaderIndex);
        }

        public void RefreshShaderList()
        {
            if (collection == null)
            {
                m_RightContainer?.RemoveFromHierarchy();
                return;
            }
            m_CollectionMapper = new ShaderVariantCollectionMapper(collection);
            DrawRightContainer();
            m_ShaderList?.SetSelection(m_OldSelectedShaderIndex);
        }

        private void DrawLeftContainer()
        {
            m_LeftContainer?.RemoveFromHierarchy();
            // create scroll view
            m_LeftContainer = new ScrollView();
            m_LeftContainer.AddToClassList(leftContainerClass);
            m_MainContainer.Add(m_LeftContainer);

            m_ShaderVariantCollectionField = new ObjectField("配置文件:");
            m_ShaderVariantCollectionField.AddToClassList(shaderCollectionAssetsClass);
            m_ShaderVariantCollectionField.objectType = typeof(ShaderCollectionAssets);
            m_ShaderVariantCollectionField.value = AssetDatabase.FindAssets("t:ShaderCollectionAssets")
                .Select(guid => AssetDatabase.LoadAssetAtPath<ShaderCollectionAssets>(AssetDatabase.GUIDToAssetPath(guid)))
                .FirstOrDefault();
            m_ShaderVariantCollectionField.RegisterValueChangedCallback((evt) =>
            {
                var shaderCollectionAssets = evt.newValue as ShaderCollectionAssets;
                UpdateShaderCollectionAssetsUI(shaderCollectionAssets);
            });

            m_LeftContainer.Add(m_ShaderVariantCollectionField);
            m_ConfigContainerBox = new VisualElement();
            m_LeftContainer.Add(m_ConfigContainerBox);
            UpdateShaderCollectionAssetsUI(m_ShaderVariantCollectionField.value as ShaderCollectionAssets);



            var createConfigAsset = new Button(() =>
            {
                var configAssets = ScriptableObject.CreateInstance<ShaderCollectionAssets>();
                var path = "Assets/ShaderCollectionAssets.asset";
                path = AssetDatabase.GenerateUniqueAssetPath(path);
                AssetDatabase.CreateAsset(configAssets, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                m_ShaderVariantCollectionField.value = configAssets;
                Selection.activeObject = configAssets;
            })
            {
                text = "New"
            };
            m_ShaderVariantCollectionField.Add(createConfigAsset);
        }

        // 绘制右边的shader列表，shader根据m_SVCField的值来获取，根据ShaderVariantCollection获取所有的shader
        private void DrawRightContainer()
        {
            m_RightContainer?.RemoveFromHierarchy();

            m_RightContainer = new TwoPaneSplitView(0, 350, TwoPaneSplitViewOrientation.Horizontal);
            m_MainContainer.Add(m_RightContainer);

            if (m_SVCField.value == null)
                return;

            m_ShaderContainer = new VisualElement();
            m_ShaderContainer.AddToClassList(shaderContainerClass);
            m_RightContainer.Add(m_ShaderContainer);

            UpdateShaderListUI();

            m_VariantContainer = new VisualElement();
            m_VariantContainer.AddToClassList(shaderVariantContainerClass);
            m_RightContainer.Add(m_VariantContainer);

            m_RightContainer.fixedPaneIndex = 0;
        }

        List<Shader> m_ShaderListData = new List<Shader>();
        // Update Shader List UI
        private void UpdateShaderListUI()
        {
            m_ShaderListData = collectionMapper.shaders.ToList();
            // create title
            var title = new Label($"Shader列表({m_ShaderListData.Count})");
            title.AddToClassList(shaderContainerTitleClass);
            m_ShaderContainer.Add(title);

            // create sort button
            var sortBox = new VisualElement();
            sortBox.AddToClassList(sortBoxClass);
            var sortButton1 = new Button(() =>
            {
                SortShaderList(0);
            })
            {
                text = "按名称排序"
            };
            sortButton1.AddToClassList(sortButtonClass);
            var sortButton2 = new Button(() =>
            {
                SortShaderList(1);
            })
            {
                text = "按变体数量排序"
            };
            sortButton2.AddToClassList(sortButtonClass);
            sortBox.Add(sortButton1);
            sortBox.Add(sortButton2);
            m_ShaderContainer.Add(sortBox);


            Func<VisualElement> makeItem = () =>
            {
                var box = new VisualElement();
                box.AddToClassList(shaderItemBoxClass);

                var shaderName = new Label() { name = "shaderName" };
                box.Add(shaderName);

                var variantCount = new Label() { name = "variantCount" };
                variantCount.AddToClassList(variantCountClass);
                box.Add(variantCount);
                return box;
            };

            Action<VisualElement, int> bindItem = (e, i) =>
            {
                var shader = m_ShaderListData[i];
                var variants = collectionMapper.GetShaderVariants(shader);

                e.tooltip = $"{shader.name}\n{variants.Count}个变体";
                // 获取label和button
                var shaderName = e.Q<Label>(name: "shaderName");
                var variantCount = e.Q<Label>(name: "variantCount");
                shaderName.text = shader.name;
                variantCount.text = variants.Count.ToString();
            };

            const int itemHeight = 20;
            m_ShaderList = new ListView(m_ShaderListData, itemHeight, makeItem, bindItem);
            m_ShaderList.reorderable = true;
            m_ShaderList.reorderMode = ListViewReorderMode.Animated;
            m_ShaderList.showBorder = true;
            m_ShaderList.showAddRemoveFooter = true;
            m_ShaderList.showAlternatingRowBackgrounds = AlternatingRowBackground.None;

            m_ShaderList.showBoundCollectionSize = true;
            m_ShaderList.selectionType = SelectionType.Single;
            m_ShaderList.onItemsChosen += OnItemsChosen;
            m_ShaderList.onSelectionChange += OnSelectShader;
            var addButton = m_ShaderList.Q<Button>(name: "unity-list-view__add-button");
            if (addButton != null) addButton.clickable = new Clickable(OnAddShaderClicked);


            var removeButton = m_ShaderList.Q<Button>(name: "unity-list-view__remove-button");
            if (removeButton != null) removeButton.clickable = new Clickable(OnRemoveShaderClicked);

            // unity-list-view__footer
            m_NewShaderField = new ObjectField("Shader:")
            {
                objectType = typeof(Shader),
                name = "Add Shader"
            };
            m_ShaderList.hierarchy.Add(m_NewShaderField);

            // 移动buttons到shaderField 中
            var buttons = m_ShaderList.Q(className: "unity-list-view__footer");
            buttons.RemoveFromHierarchy();
            m_NewShaderField.Add(buttons);

            m_ShaderContainer.Add(m_ShaderList);

        }

        private void OnRemoveShaderClicked(EventBase evt)
        {
            var shader = m_ShaderList.selectedItem as Shader;
            collectionMapper.RemoveShader(shader);
            m_ShaderListData.Remove(shader);
            // m_ShaderList.RemoveAt(m_ShaderList.selectedIndex);
            m_ShaderList.Rebuild();
        }

        private void OnAddShaderClicked(EventBase evt)
        {
            // 弹窗ObjectField选择shader，添加到shader list中
            var shader = m_NewShaderField.value as Shader;
            if (shader)
            {
                if (collectionMapper.HasShader(shader))
                {
                    EditorUtility.DisplayDialog("提示", "已经存在该Shader", "确定");
                }
                else
                {
                    collectionMapper.AddShader(shader);
                    m_ShaderListData.Add(shader);
                    m_ShaderList.Rebuild();
                }
            }
            else
            {
                EditorUtility.DisplayDialog("提示", "请选择一个Shader", "确定");
            }
        }

        private void OnSelectShader(IEnumerable<object> enumerable)
        {
            var m_activeItem = enumerable.FirstOrDefault() as Shader;
            UpdateShaderVariantListUI(m_activeItem);
            m_OldSelectedShaderIndex = m_SelectedShaderIndex;
        }
        // 双击
        private void OnItemsChosen(IEnumerable<object> enumerable)
        {
            var selectedShader = enumerable.FirstOrDefault() as Shader;
            EditorGUIUtility.PingObject(selectedShader);
        }

        bool revertSort = false;
        // sort shader list by shader name, shader variant count
        private void SortShaderList(int index)
        {
            revertSort = !revertSort;
            if (index == 0)
            {
                m_ShaderListData.Sort((a, b) =>
                {
                    var shaderA = a as Shader;
                    var shaderB = b as Shader;
                    if (shaderA == null || shaderB == null)
                    {
                        return 0;
                    }
                    var shaderAName = shaderA.name;
                    var shaderBName = shaderB.name;
                    // 根据revertSort来决定升序还是降序
                    return revertSort ? shaderAName.CompareTo(shaderBName) : shaderBName.CompareTo(shaderAName);
                });
            }
            else if (index == 1)
            {
                // 根据shader variant count排序
                m_ShaderListData.Sort((a, b) =>
                {
                    var shaderA = a as Shader;
                    var shaderB = b as Shader;
                    if (shaderA == null || shaderB == null)
                    {
                        return 0;
                    }
                    var shaderAVariants = collectionMapper.GetShaderVariants(shaderA);
                    var shaderBVariants = collectionMapper.GetShaderVariants(shaderB);
                    // 根据revertSort来决定升序还是降序
                    return revertSort ? shaderAVariants.Count.CompareTo(shaderBVariants.Count) : shaderBVariants.Count.CompareTo(shaderAVariants.Count);
                });
            }
            m_ShaderList.Rebuild();
        }
        // 缓存下VariantDict 用来更新UI
        private Dictionary<PassType, List<ShaderVariant>> m_VariantDict;
        private void UpdateShaderVariantListUI(Shader currentShader)
        {
            m_VariantContainer.Clear();
            m_VariantDict = collectionMapper.GetShaderVariantDict(currentShader);
            foreach (var variantDict in m_VariantDict)
            {
                Func<VisualElement> makeItem = () =>
                {
                    var box = new VisualElement();
                    box.AddToClassList(shaderItemBoxClass);
                    box.style.fontSize = 10;
                    var label = new Label();
                    box.Add(label);
                    return box;
                };

                Action<VisualElement, int> bindItem = (e, i) =>
                {
                    var shaderVariant = variantDict.Value[i];
                    var shaderKeywordsStr = shaderVariant.keywords.Length == 0 ? "<No Keywords>" : string.Join("  ", shaderVariant.keywords);
                    var label = e.Q<Label>();
                    label.text = shaderKeywordsStr;
                    e.tooltip = shaderKeywordsStr;
                    e.userData = shaderVariant;
                };

                var variantListView = new ListView(variantDict.Value, 20, makeItem, bindItem);
                variantListView.showFoldoutHeader = true;
                variantListView.headerTitle = variantDict.Key.ToString();
                variantListView.reorderable = true;
                variantListView.reorderMode = ListViewReorderMode.Animated;
                variantListView.showBorder = true;
                variantListView.showAddRemoveFooter = true;
                variantListView.showAlternatingRowBackgrounds = AlternatingRowBackground.None;
                variantListView.showBoundCollectionSize = true;
                variantListView.selectionType = SelectionType.Single;
                variantListView.userData = variantDict.Key;
                variantListView.horizontalScrollingEnabled = true;
                var addButton = variantListView.Q<Button>(name: "unity-list-view__add-button");
                // if (addButton != null) addButton.style.display = DisplayStyle.None;
                if (addButton != null) addButton.clickable = new Clickable(OnAddVariantClicked);

                var removeButton = variantListView.Q<Button>(name: "unity-list-view__remove-button");
                if (removeButton != null) removeButton.clickable = new Clickable(OnRemoveVariantClicked);


                m_VariantContainer.Add(variantListView);
            }

            var addButton2 = new Button(() =>
            {
                ShaderVariantAddWindow.ShowWindow(CurrentSelectedShader, PassType.ScriptableRenderPipeline, collectionMapper);
            })
            {
                text = "+"
            };
            m_VariantContainer.Add(addButton2);
        }


        // On Add Variant Clicked
        private void OnAddVariantClicked(EventBase evt)
        {
            var button = evt.target as Button;
            var listView = button.GetFirstAncestorOfType<ListView>();
            if (listView.userData == null)
            {
                return;
            }

            ShaderVariantAddWindow.ShowWindow(CurrentSelectedShader, (PassType)listView.userData, collectionMapper);
        }

        // On Remove Variant Clicked
        private void OnRemoveVariantClicked(EventBase evt)
        {
            var button = evt.target as Button;
            var listView = button.GetFirstAncestorOfType<ListView>();
            if (listView.selectedItem == null)
            {
                EditorUtility.DisplayDialog("提示", "请选择一个变体", "确定");
                return;
            }
            var shaderVariant = (ShaderVariant)listView.selectedItem;

            if (RemoveShaderVariant(shaderVariant))
                m_VariantContainer.Remove(listView);
            else
                listView?.Rebuild();

            UpdateShaderVariantCountLabel();

        }


        // update shader variant count label
        private void UpdateShaderVariantCountLabel()
        {
            var selectedVisualElement = m_ShaderList.Q<VisualElement>(className: "unity-collection-view__item--selected");
            var variantCount = selectedVisualElement.Q<Label>(name: "variantCount");

            var variants = collectionMapper.GetShaderVariants(m_ShaderList.selectedItem as Shader);

            variantCount.text = variants.Count.ToString();
        }

        // remove shader variant
        private bool RemoveShaderVariant(ShaderVariant shaderVariant)
        {
            collectionMapper.RemoveVariant(shaderVariant);
            // try get variants
            if (m_VariantDict.TryGetValue(shaderVariant.passType, out var variants))
            {
                variants.Remove(shaderVariant);
                if (variants.Count == 0)
                {
                    m_VariantDict.Remove(shaderVariant.passType);
                    return true;
                }
            }
            return false;
        }

        // 更新shaderCollectionAssets的UI
        private void UpdateShaderCollectionAssetsUI(ScriptableObject shaderCollectionAssets)
        {
            m_ConfigContainerBox.Clear();
            if (shaderCollectionAssets == null)
                return;
            m_ConfigContainerBox.Add(shaderCollectionAssets.CreateUIElementInspector());
        }

        private void OnDisable()
        {
            // if (m_ShaderVariantCollectionField != null)
            // {
            //     EditorUtility.SetDirty(m_ShaderVariantCollectionField);
            //     AssetDatabase.SaveAssets();
            // }
        }

    }


}
