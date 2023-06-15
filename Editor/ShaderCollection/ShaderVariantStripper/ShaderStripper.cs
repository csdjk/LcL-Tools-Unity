using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEditor.Rendering;
using System.Collections.Generic;
using UnityEditor.Build;
using System.Linq;
using System.IO;
using UnityEditor.Build.Reporting;
using System;

namespace LcLTools
{
    // Shader剔除
    class ShaderStripper : IPreprocessShaders, IPreprocessBuildWithReport
    {
        public int callbackOrder
        {
            get { return 0; } // 可以指定多个处理器之间回调的顺序
        }
        private readonly static string ShaderVariantStripperOutput = "ShaderVariantStripperOutput.txt";
        private readonly static string ShaderVariantBuildOutput = "ShaderVariantBuildOutput.txt";
        private static string[] path = { "Assets" };
        private static ShaderStripperAssets[] m_Configs;
        public static void InitShaderStripperAssets()
        {
            string[] guids = AssetDatabase.FindAssets("t:ShaderStripperAssets", path);
            m_Configs = (from guid in guids
                         select AssetDatabase.LoadAssetAtPath<ShaderStripperAssets>(
                             AssetDatabase.GUIDToAssetPath(guid)))
                        .ToArray();
        }

        public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
        {
            InitShaderStripperAssets();
            foreach (var config in m_Configs)
            {
                if (config.Active)
                {
                    var result = config.ValidShaderVariants(shader, snippet, data);
                    for (int i = data.Count - 1; i >= 0; --i)
                    {
                        string shaderName = shader.name;
                        string shaderVariants = string.Join(", ", data[i].shaderKeywordSet.GetShaderKeywords().ToArray());
                        string output = $"{shaderName}: {shaderVariants}\n";
                        if (result[i])
                        {
                            File.AppendAllText(ShaderVariantStripperOutput, output);
                            Debug.Log("剔除Shader变体：" + output);
                            data.RemoveAt(i);
                        }
                        else
                        {
                            File.AppendAllText(ShaderVariantBuildOutput, output);
                        }
                    }
                }
            }
        }

        // init function
        static public void Init()
        {
            InitShaderStripperAssets();
            // 清空输出文件
            File.WriteAllText(ShaderVariantStripperOutput, "");
            File.WriteAllText(ShaderVariantBuildOutput, "");

            // Write the current time to a file
            File.AppendAllText(ShaderVariantStripperOutput, $"Time: {DateTime.Now}\n");
            File.AppendAllText(ShaderVariantBuildOutput, $"Time: {DateTime.Now}\n");
        }
        public void OnPreprocessBuild(BuildReport report)
        {
            Init();
        }
    }
}