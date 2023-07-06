using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.Rendering;
using UnityEditor.Rendering;
using System;
namespace LcLTools
{
    [Serializable]
    public class VariantStripData
    {
        public string keyword;
        public List<Shader> reservedShaderList;
        public VariantStripData() { }

        public VariantStripData(string keyword)
        {
            this.keyword = keyword;
        }
    }

    [CreateAssetMenu(fileName = "ShaderStripperAssets", menuName = "LcL/ShaderVariantTools/ShaderStripperAssets", order = 0)]
    public class ShaderStripperAssets : ScriptableObject
    {

        [Button("激活")]
        public bool Active = false;

        [Rename("剔除整个Shader", "")]
        public List<Shader> shaderExcludeShaderList;

        // 剔除shader PassType 列表
        [Rename("剔除PassType", "")]
        public List<PassType> shaderExcludePassTypeList = new List<PassType>() { PassType.Meta };


        // [Rename("剔除变体", "")]
        // public List<string> variantExcludeList = new List<string>() { "STEREO_INSTANCING_ON", "STEREO_MULTIVIEW_ON", "STEREO_CUBEMAP_RENDER_ON", "UNITY_SINGLE_PASS_STEREO", "EDITOR_VISUALIZATION" };

        [SerializeReference]
        public List<VariantStripData> variantExcludeList = new List<VariantStripData>() {
        new VariantStripData("STEREO_INSTANCING_ON"),
        new VariantStripData("STEREO_MULTIVIEW_ON"),
        new VariantStripData("STEREO_CUBEMAP_RENDER_ON"),
        new VariantStripData("UNITY_SINGLE_PASS_STEREO"),
        new VariantStripData("EDITOR_VISUALIZATION"),
    };
        private void OnEnable()
        {
            Debug.Log("OnEnable");
        }
        public ShaderStripperAssets()
        {
            Debug.Log("ShaderStripperAssets");
        }

        // 判断shader是否排除shader列表里面
        public bool IsInShaderExcludeShaderList(Shader shader)
        {
            if (shaderExcludeShaderList == null || shaderExcludeShaderList.Count == 0)
            {
                return false;
            }
            return shaderExcludeShaderList.Contains(shader);
        }

        // 判断shader是否排除PassType列表里面
        public bool IsInShaderExcludePassTypeList(PassType passType)
        {
            if (shaderExcludePassTypeList == null || shaderExcludePassTypeList.Count == 0)
            {
                return false;
            }
            return shaderExcludePassTypeList.Contains(passType);
        }
        /// <summary>
        /// 剔除结果
        /// </summary>
        /// <typeparam name="bool"> true：剔除，false：保留</typeparam>
        private List<bool> strippingResult = new List<bool>();
        public List<bool> ValidShaderVariants(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
        {
            strippingResult.Clear();
            // 剔除整个Shader
            if (IsInShaderExcludeShaderList(shader) || IsInShaderExcludePassTypeList(snippet.passType))
            {
                foreach (var item in data)
                {
                    strippingResult.Add(true);
                }
            }
            else
            {
                foreach (var item in data)
                {
                    strippingResult.Add(IsInVariantExcludeList(item.shaderKeywordSet, shader));
                }
            }
            return strippingResult;
        }

        // 判断变体是否在排除变体列表里面
        public bool IsInVariantExcludeList(ShaderKeywordSet keywordSet, Shader shader)
        {
            if (variantExcludeList == null || variantExcludeList.Count == 0)
            {
                return false;
            }
            foreach (var item in variantExcludeList)
            {
                // 当这个变体开启了keyword，并且shader不在保留shader列表里面，就剔除。
                if (keywordSet.IsEnabled(new ShaderKeyword(item.keyword)) && !item.reservedShaderList.Contains(shader))
                {
                    return true;
                }
            }
            return false;
        }

    }
}