
using UnityEngine;
using UnityEditor;
using DodElements;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System.IO;

namespace LcLTools
{

    public class ShaderCollectionWindow : EditorWindow
    {
        // 
        private const string shaderVariantCollectionName = "AllShaders.shadervariants";
        private const string defaultShaderVariantCollectionPath = "Assets/Resources/Shaders/" + shaderVariantCollectionName;

        private List<string> includeFolderList = new List<string>(){
            "Assets",
        };
        private List<string> excludeFolderList = new List<string>(){
            "LiChangLong",
        };
        private List<string> excludeShaderList = new List<string>() { };


        [MenuItem("LcLTools/Shader变体收集")]
        private static void ShowWindow()
        {
            var window = GetWindow<ShaderCollectionWindow>();
            window.titleContent = new GUIContent("ShaderCollectionWindow");
            window.Show();
        }

        private void OnEnable()
        {
            VisualElement root = rootVisualElement;

            // title label
            var title = new Label("Shader变体收集");
            title.style.fontSize = 20;
            title.style.unityTextAlign = TextAnchor.MiddleCenter;
            title.style.marginTop = 10;
            title.style.marginBottom = 10;
            root.Add(title);

            var include = new TextFieldList("包含的文件夹:", includeFolderList);
            root.Add(include);

            var exclude = new TextFieldList("排除的文件夹:", excludeFolderList);
            root.Add(exclude);

            var excludeShader = new TextFieldList("排除的Shader:", excludeShaderList);
            root.Add(excludeShader);

            var folderText = new FolderTextField("ShaderVariantCollectionPath", defaultShaderVariantCollectionPath);
            folderText.RegisterValueChangedCallback((evt) =>
            {
                string path = evt.newValue;
                if (!path.EndsWith(".shadervariants"))
                {
                    path = Path.Combine(path, shaderVariantCollectionName);
                    folderText.value = LcLUtility.AssetsRelativePath(path);
                }
            });
            folderText.style.marginLeft = 20;
            folderText.style.marginRight = 20;
            root.Add(folderText);

            var collect = new Button(() =>
            {
                ShaderCollection.ALL_SHADER_VARAINT_ASSET_PATH = folderText.value;
                Debug.Log("ShaderVariantCollectionPath:" + ShaderCollection.ALL_SHADER_VARAINT_ASSET_PATH);
                ShaderCollection.CollectShaderVariant(includeFolderList.ToArray(), excludeFolderList.ToArray(), excludeShaderList.ToArray());
            })
            {
                text = "Shader变体收集"
            };
            collect.style.marginLeft = 20;
            collect.style.marginRight = 20;
            collect.style.marginTop = 20;
            collect.style.height = 50;
            root.Add(collect);
        }
    }
}