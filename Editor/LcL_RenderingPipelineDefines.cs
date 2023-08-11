
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace LcLTools
{
    public enum LcL_PipelineType
    {
        Unsupported,
        BiRP,
        URP,
        HDRP
    }

    [InitializeOnLoad]
    public class LcL_RenderingPipelineDefines
    {

        static LcL_RenderingPipelineDefines()
        {
            InitDefines();
        }

        static void InitDefines()
        {

        }


        /// <summary>
        /// 获取当前管道类型
        /// </summary>
        /// <returns></returns>
        public static LcL_PipelineType GetPipeline()
        {
#if UNITY_2019_1_OR_NEWER
            if (GraphicsSettings.renderPipelineAsset != null)
            {
                // SRP
                var srpType = GraphicsSettings.renderPipelineAsset.GetType().ToString();
                if (srpType.Contains("HDRenderPipelineAsset"))
                {
                    return LcL_PipelineType.HDRP;
                }
                else if (srpType.Contains("UniversalRenderPipelineAsset") || srpType.Contains("LightweightRenderPipelineAsset"))
                {
                    return LcL_PipelineType.URP;
                }
                else return LcL_PipelineType.Unsupported;
            }
#elif UNITY_2017_1_OR_NEWER
        if (GraphicsSettings.renderPipelineAsset != null) {
            // SRP not supported before 2019
            return HEU_PipelineType.Unsupported;
        }
#endif
            return LcL_PipelineType.BiRP;
        }

        /// <summary>
        ///  添加宏定义
        /// </summary>
        /// <param name="define"></param> <summary>
        public static void AddDefine(string define)
        {
            var definesList = GetDefines();
            if (!definesList.Contains(define))
            {
                definesList.Add(define);
                SetDefines(definesList);
            }
        }

        /// <summary>
        /// 移除宏定义
        /// </summary>
        /// <param name="define"></param>
        public static void RemoveDefine(string define)
        {
            var definesList = GetDefines();
            if (definesList.Contains(define))
            {
                definesList.Remove(define);
                SetDefines(definesList);
            }
        }

        public static List<string> GetDefines()
        {

#if UNITY_EDITOR
            var target = EditorUserBuildSettings.activeBuildTarget;
            var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(target);
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            return defines.Split(';').ToList();
#else
        return new List<string>();
#endif
        }

        public static void SetDefines(List<string> definesList)
        {
#if UNITY_EDITOR
            var target = EditorUserBuildSettings.activeBuildTarget;
            var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(target);
            var defines = string.Join(";", definesList.ToArray());
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);
#endif
        }
    }
}