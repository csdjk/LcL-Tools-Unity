using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using LcLTools;

namespace LcLTools
{
    public class ShaderCollection
    {
        readonly public static string[] IncludeFolderList = new string[] { "Assets" };
        private ShaderVariantCollection svc;

        public static string ALL_SHADER_VARAINT_ASSET_PATH = "Assets/Resources/Shaders/AllShaders.shadervariants";
        static string toolsSVCpath = "Assets/Resources/Shaders/Tools.shadervariants";
        static ShaderVariantCollection ToolSVC = null;
        static List<string> allShaderNameList = new List<string>();


        public static void CollectShaderVariantFormCustomsList(string[] allAssets, string[] excludeShaderList = null)
        {
            //收集材质
            string[] allMatPaths = allAssets.Where((asset) => asset.EndsWith(".mat", StringComparison.OrdinalIgnoreCase)).ToArray();
            CollectShaderVariantFormMaterials(allMatPaths, excludeShaderList);
        }

        public static void CollectShaderVariant(string[] includeFolderList = null, string[] excludeFolderList = null, string[] excludeShaderList = null)
        {
            string[] allMatPaths = CollectMatFromAssets(includeFolderList, excludeFolderList);
            CollectShaderVariantFormMaterials(allMatPaths, excludeShaderList);
        }

        /// <summary>
        /// 简单收集
        /// </summary>
        public static void CollectShaderVariantFormMaterials(string[] allMatPaths, string[] excludeShaderList = null)
        {
            //创建上下文
            //先搜集所有keyword到工具类SVC
            ToolSVC = new ShaderVariantCollection();
            var shaders = AssetDatabase.FindAssets("t:Shader", new string[] { "Assets", "Packages" }).ToList();
            foreach (var guid in shaders)
            {
                var shaderPath = AssetDatabase.GUIDToAssetPath(guid);
                // if (shaderPath.Contains("Lit"))
                // {
                //     Debug.Log($"跳过shader:{shaderPath}");
                //     continue;
                // }
                //var shader = AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);
                //清理shader的默认图片
                Shader shader = null;
                bool ischanged = false;
                var ai = AssetImporter.GetAtPath(shaderPath);
                if (ai is ShaderImporter shaderImporter)
                {
                    shader = shaderImporter.GetShader();

                    for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
                    {
                        var type = ShaderUtil.GetPropertyType(shader, i);
                        if (type == ShaderUtil.ShaderPropertyType.TexEnv)
                        {
                            var propName = ShaderUtil.GetPropertyName(shader, i);
                            var tex = shaderImporter.GetDefaultTexture(propName);
                            if (tex)
                            {
                                ischanged = true;
                                shaderImporter.SetDefaultTextures(new string[] { propName }, new Texture[] { null });
                                Debug.Log($"清理shader默认贴图:{shaderPath} - {propName}");
                            }
                        }
                    }
                }
                else
                {
                    shader = AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);
                }

                if (ischanged)
                {
                    ai.SaveAndReimport();
                }


                //添加
                ShaderVariantCollection.ShaderVariant sv = new ShaderVariantCollection.ShaderVariant();
                sv.shader = shader;
                ToolSVC.Add(sv);

                allShaderNameList.Add(shaderPath);
            }


            //防空
            var dirt = Path.GetDirectoryName(toolsSVCpath);
            if (!Directory.Exists(dirt))
            {
                Directory.CreateDirectory(dirt);
            }

            AssetDatabase.CreateAsset(ToolSVC, toolsSVCpath);

            //开始收集ShaderVaraint
            ShaderVariantCollection allShaderVaraint = null;
            var tools = new ShaderVariantsCollectionTools(excludeShaderList);
            allShaderVaraint = tools.CollectionKeywords(allMatPaths.ToArray(), ToolSVC);

            //输出SVC文件
            var targetDir = Path.GetDirectoryName(ALL_SHADER_VARAINT_ASSET_PATH);
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            AssetDatabase.DeleteAsset(ALL_SHADER_VARAINT_ASSET_PATH);
            AssetDatabase.CreateAsset(allShaderVaraint, ALL_SHADER_VARAINT_ASSET_PATH);
            AssetDatabase.Refresh();

            Debug.Log("<color=red>shader_features收集完毕,multi_compiles默认全打包需要继承IPreprocessShaders.OnProcessShader自行剔除!</color>");
            // var dependencies = AssetDatabase.GetDependencies(ALL_SHADER_VARAINT_ASSET_PATH);
            // foreach (var guid in dependencies )
            // {
            //     Debug.Log("依赖shader:" + guid);
            // }
        }


        /// <summary>
        /// 收集所有资源中的mat
        /// </summary>
        /// <returns></returns>
        static private string[] CollectMatFromAssets(string[] includeFolderList = null, string[] excludeFolderList = null)
        {
            includeFolderList = includeFolderList ?? IncludeFolderList;
            //搜索所有资源中所有可能挂载mat的地方
            var scriptObjectAssets = AssetDatabase.FindAssets("t:ScriptableObject", includeFolderList).ToList(); //自定义序列化脚本中也有可能有依赖
            var prefabAssets = AssetDatabase.FindAssets("t:Prefab", includeFolderList).ToList();
            var matAssets = AssetDatabase.FindAssets("t:Material", includeFolderList).ToList();

            //搜索mat
            var guidList = new List<string>();
            guidList.AddRange(prefabAssets);
            guidList.AddRange(matAssets);
            guidList.AddRange(scriptObjectAssets);
            List<string> allMatPaths = new List<string>();
            //GUID to assetPath
            for (int i = 0; i < guidList.Count; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guidList[i]);
                if (IsExcludePath(path, excludeFolderList))
                {
                    Debug.Log("排除路径:" + path);
                    continue;
                }
                //获取依赖中的mat
                var dependenciesPath = AssetDatabase.GetDependencies(path, true);
                foreach (var dp in dependenciesPath)
                {
                    if (Path.GetExtension(dp).Equals(".mat", StringComparison.OrdinalIgnoreCase))
                    {
                        allMatPaths.Add(dp);
                    }
                    else if (Path.GetExtension(dp).Equals(".asset", StringComparison.OrdinalIgnoreCase)) //依赖的ScripttableObject,会
                    {
                        scriptObjectAssets.Add(LcLEditorUtilities.AssetPathToGUID(dp));
                    }
                }
            }

            //ScripttableObject 里面有可能存mat信息
            foreach (var asset in scriptObjectAssets)
            {
                var path = AssetDatabase.GUIDToAssetPath(asset);
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat != null)
                {
                    allMatPaths.Add(path);
                }
            }


            return allMatPaths.Distinct().ToArray();
        }

        private static bool IsExcludePath(string path, string[] excludeFolderList)
        {
            if (excludeFolderList == null || excludeFolderList.Length == 0)
            {
                return false;
            }
            foreach (var exclude in excludeFolderList)
            {
                if (path.Contains(exclude))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 打包ShaderOnly
        /// </summary>
        // public static void BuildShadersAssetBundle()
        // {
        //     AssetDatabase.StartAssetEditing();
        //     {
        //         CollectShaderVariant();
        //         var guid = LcLEditorUtilities.AssetPathToGUID(ALL_SHADER_VARAINT_ASSET_PATH);

        //         List<AssetImporter> list = new List<AssetImporter>();
        //         //依赖信息
        //         var dependice = AssetDatabase.GetDependencies(ALL_SHADER_VARAINT_ASSET_PATH);
        //         foreach (var depend in dependice)
        //         {
        //             var type = AssetDatabase.GetMainAssetTypeAtPath(depend);
        //             if (type == typeof(Material) || type == typeof(ShaderVariantCollection))
        //             {
        //                 var ai = AssetImporter.GetAtPath(depend);
        //                 ai.SetAssetBundleNameAndVariant(guid, null);
        //                 Debug.Log("打包:" + depend);
        //                 list.Add(ai);
        //             }
        //         }

        //         //开始编译
        //         var outpath = IPath.Combine(BApplication.Library, "BDBuildTest", BApplication.GetRuntimePlatformPath());
        //         if (Directory.Exists(outpath))
        //         {
        //             Directory.Delete(outpath, true);
        //         }

        //         Directory.CreateDirectory(outpath);
        //         //
        //         var buildtarget = BApplication.GetBuildTarget(BApplication.RuntimePlatform);
        //         UnityEditor.BuildPipeline.BuildAssetBundles(outpath, BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.ChunkBasedCompression, buildtarget);
        //         //
        //         foreach (var ai in list)
        //         {
        //             ai.SetAssetBundleNameAndVariant(null, null);
        //         }

        //         Debug.Log("测试AB已经输出:" + outpath);
        //     }
        //     AssetDatabase.StopAssetEditing();
        // }
    }
}
