using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.Rendering;
namespace LcLTools
{

    [CreateAssetMenu(fileName = "ShaderCollectionAssets", menuName = "LcL/ShaderVariantTools/ShaderCollectionAssets", order = 0)]
    public class ShaderCollectionAssets : ScriptableObject
    {
        [Rename("只在以下目录资源收集Shader变体", "")]
        public List<DefaultAsset> assetsIncludeFolderList;
        private List<string> _assetsIncludeFolderList = new List<string>();


        [Rename("搜索资源时排除以下文件夹", "")]
        public List<DefaultAsset> assetsExcludeFolderList;
        private List<string> _assetsExcludeFolderList = new List<string>();


        [Rename("只收集以下目录的Shader变体", "")]
        public List<DefaultAsset> shaderIncludeFolderList;


        [Rename("排除以下Shader", "")]
        public List<Shader> shaderExcludeShaderList;

        [Rename("剔除以下PassType", "")]
        public List<PassType> passTypeExcludeList = new List<PassType>() { PassType.Meta };


        // [Rename("排除以下资源的引用Shader", "")]
        // public List<Object> assetsExcludeShaderList;



        public string[] GetAssetsIncludeFolderList()
        {
            _assetsIncludeFolderList.Clear();

            if (assetsIncludeFolderList == null || assetsIncludeFolderList.Count == 0)
            {
                return new string[] { "Assets" };
            }

            foreach (var folder in assetsIncludeFolderList)
            {
                var path = AssetDatabase.GetAssetPath(folder);
                _assetsIncludeFolderList.Add(path);
            }
            return _assetsIncludeFolderList.ToArray();
        }

        /// <summary>
        /// 判断当前资源是否通过
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public bool IsPass(string assetPath)
        {
            if (assetsExcludeFolderList == null) return true;
            foreach (var folder in assetsExcludeFolderList)
            {
                var path = AssetDatabase.GetAssetPath(folder);
                if (assetPath.Contains(path))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 判断Pass是否通过
        /// </summary>
        /// <param name="passType"></param>
        /// <returns></returns>
        public bool IsPass(PassType passType)
        {
            return !passTypeExcludeList.Contains(passType);
        }
        /// <summary>
        /// 判断Shader是否通过,条件包括：1.是否在排除列表里面 2.是否在必须包含的文件夹列表里面
        /// </summary>
        /// <param name="shader"></param>
        /// <returns></returns>
        public bool IsPass(Shader shader)
        {
            var shaderPath = AssetDatabase.GetAssetPath(shader);

            if (!IsInShaderIncludeFolderList(shaderPath))
            {
                return false;
            }

            if (IsInShaderExcludeShaderList(shader))
            {
                return false;
            }

            return true;
        }


        // 判断shader是否在必须包含的文件夹列表里面
        private bool IsInShaderIncludeFolderList(string shaderPath)
        {
            if (shaderIncludeFolderList == null || shaderIncludeFolderList.Count == 0)
            {
                return true;
            }
            foreach (var folder in shaderIncludeFolderList)
            {
                var path = AssetDatabase.GetAssetPath(folder);
                if (shaderPath.Contains(path))
                {
                    return true;
                }
            }
            return false;
        }
        // 判断shader是否排除shader列表里面
        private bool IsInShaderExcludeShaderList(Shader shader)
        {
            if (shaderExcludeShaderList == null || shaderExcludeShaderList.Count == 0)
            {
                return false;
            }
            return shaderExcludeShaderList.Contains(shader);
        }


        private void OnEnable()
        {
            Debug.Log("OnEnable");
        }
    }
}