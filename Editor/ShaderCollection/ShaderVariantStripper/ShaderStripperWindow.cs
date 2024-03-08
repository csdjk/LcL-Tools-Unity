
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

    public class ShaderStripperWindow : EditorWindow
    {
        public static StyleSheet styleSheet => LcLEditorUtilities.GetStyleSheet("ShaderCollection");

        private const string titleClass = "title";
        private const string leftContainerClass = "left-container";
        private const string rightContainerClass = "right-container";
        private const string shaderCollectionAssetsClass = "shader-collection-assets";
        private ObjectField m_ConfigField;
        private VisualElement m_LeftContainer;
        private VisualElement m_RightContainer;
        private ObjectField m_SVCField;
        private VisualElement m_ConfigContainerBox;

        [SerializeField]

        // [MenuItem("LcLTools/Shader变体剔除")]
        private static void ShowWindow()
        {
            var window = GetWindow<ShaderStripperWindow>();
            window.titleContent = new GUIContent("ShaderStripperWindow");
            window.Show();
        }

        private void OnEnable()
        {
            VisualElement root = rootVisualElement;
            root.styleSheets.Add(styleSheet);

            var title = new Label("Shader变体剔除");
            title.AddToClassList(titleClass);
            root.Add(title);

            // 创建一个左右布局的容器
            TwoPaneSplitView container = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);

            root.Add(container);


            // 左边的配置列表
            m_LeftContainer = new VisualElement();
            m_LeftContainer.AddToClassList(leftContainerClass);
            container.Add(m_LeftContainer);

            DrawLeftContainer();

            // 右边的shader列表
            m_RightContainer = new VisualElement();
            m_RightContainer.AddToClassList(rightContainerClass);
            container.Add(m_RightContainer);

            // create button
            var btn = new Button(() =>
            {
                Debug.Log((m_ConfigField.value as ShaderStripperAssets).Active);
            });
            m_LeftContainer.Add(btn);

        }

        private void DrawLeftContainer()
        {
            m_ConfigField = new ObjectField("配置文件:");
            m_ConfigField.AddToClassList(shaderCollectionAssetsClass);
            m_ConfigField.objectType = typeof(ShaderStripperAssets);
            m_ConfigField.value = AssetDatabase.FindAssets("t:ShaderStripperAssets")
                .Select(guid => AssetDatabase.LoadAssetAtPath<ShaderStripperAssets>(AssetDatabase.GUIDToAssetPath(guid)))
                .FirstOrDefault();
            m_ConfigField.RegisterValueChangedCallback((evt) =>
            {
                var shaderCollectionAssets = evt.newValue as ShaderStripperAssets;
                UpdateShaderCollectionAssetsUI(shaderCollectionAssets);
            });

            m_LeftContainer.Add(m_ConfigField);
            m_ConfigContainerBox = new VisualElement();
            m_LeftContainer.Add(m_ConfigContainerBox);
            UpdateShaderCollectionAssetsUI(m_ConfigField.value as ShaderStripperAssets);



            var createConfigAsset = new Button(() =>
            {
                var configAssets = ScriptableObject.CreateInstance<ShaderStripperAssets>();
                var path = "Assets/ShaderStripperAssets.asset";
                path = AssetDatabase.GenerateUniqueAssetPath(path);
                AssetDatabase.CreateAsset(configAssets, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                m_ConfigField.value = configAssets;
                Selection.activeObject = configAssets;
            })
            {
                text = "New"
            };
            m_ConfigField.Add(createConfigAsset);
        }

        private void UpdateShaderCollectionAssetsUI(ScriptableObject shaderCollectionAssets)
        {
            m_ConfigContainerBox.Clear();
            if (shaderCollectionAssets == null)
                return;
            m_ConfigContainerBox.Add(shaderCollectionAssets.CreateUIElementInspector());
        }
    }


}
