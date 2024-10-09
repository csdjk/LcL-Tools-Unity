using System.Collections.Generic;
using UnityEngine;

namespace LcLTools
{
    [System.Serializable]
    public class JsonListWrapper<T>
    {
        public List<T> list;
        public JsonListWrapper(List<T> list) => this.list = list;
    }

    public static class LcLUtility
    {
        /// <summary>
        /// 绝对路径转Unity工程相对路径
        /// </summary>
        /// <param name="absolutePath"></param>
        /// <returns></returns>
        public static string AssetsRelativePath(string absolutePath)
        {
            if (absolutePath.StartsWith(Application.dataPath))
            {
                return "Assets" + absolutePath.Substring(Application.dataPath.Length);
            }
            else
            {
                absolutePath = absolutePath.Replace('\\', '/');
                if (absolutePath.StartsWith(Application.dataPath))
                {
                    return "Assets" + absolutePath.Substring(Application.dataPath.Length);
                }
                Debug.LogWarning("Full path does not contain the current project's Assets folder");
                return absolutePath;
            }
        }
        /// <summary>
        /// 相对路径转绝对路径
        /// </summary>
        /// <param name="absolutePath"></param>
        /// <returns></returns>
        public static string AssetsRelativeToAbsolutePath(string path)
        {
            return Application.dataPath + path.Substring(6);
        }
    }
}
