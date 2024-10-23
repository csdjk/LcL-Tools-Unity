using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Text.RegularExpressions;

namespace LcLTools
{
#if UNITY_EDITOR

    /// <summary>
    /// 外部工具
    /// </summary>
    public class ExternalTools : Editor
    {

        /// <summary>
        /// 是否包含非图片格式
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsContainsNonPicture(string path)
        {
            path = path.ToLower();
            string pattern = @"\.(jpg|png|tga|psd|jpeg)$";
            string[] arr = path.Split(' ');
            foreach (string s in arr)
            {
                bool result = Regex.IsMatch(s, pattern);
                if (!result)
                {
                    EditorUtility.DisplayDialog("提示", "包含非图片文件!", "确定");
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 是否包含非模型格式
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsContainsNonModel(string path)
        {
            path = path.ToLower();
            string pattern = @"\.(fbx|obj)$";

            var name = Path.GetFileName(path);

            bool result = Regex.IsMatch(name, pattern);
            if (!result)
            {
                EditorUtility.DisplayDialog("提示", "包含非模型文件!", "确定");
                return true;
            }

            // string[] arr = path.Split(' ');
            // foreach (string s in arr)
            // {
            //     bool result = Regex.IsMatch(s, pattern);
            //     if (!result)
            //     {
            //         EditorUtility.DisplayDialog("提示", "包含非模型文件!", "确定");
            //         return true;
            //     }
            // }
            return false;
        }

        /// <summary>
        /// PS打开图片
        /// </summary>
        [MenuItem("Assets/LcL Open Image By PS", false, 2)]
        public static void OpenImage()
        {
            var path = String.Join(" ", LcLEditorUtilities.GetSelectionAssetPaths().ToArray());
            if (IsContainsNonPicture(path)) return;
            InvokeExe("Photoshop.exe", path);
        }

        [MenuItem("Assets/LcL Open Model By Houdini", false, 2)]
        public static void OpenModel()
        {
            string appPath;
            if (LcLEditorUtilities.GetApplicationByStartMenu("houdini", out appPath))
            {
                var path = String.Join(" ", LcLEditorUtilities.GetSelectionAssetPaths().ToArray());
                Debug.Log(path);
                if (IsContainsNonModel(path)) return;
                InvokeExe(appPath, path);
            }
        }

        /// <summary>
        /// 调用Exe
        /// </summary>
        private static void InvokeExe(string exe, string arguments)
        {
            UnityEngine.Debug.Log(arguments);
            AssetDatabase.Refresh();
            new Thread(new ThreadStart(() =>
            {
                try
                {
                    Process p = new Process();
                    p.StartInfo.FileName = exe;
                    // 多个参数用空格隔开
                    p.StartInfo.Arguments = arguments;
                    p.Start(); //启动程序
                    p.WaitForExit();//等待程序执行完退出进程
                    p.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            })).Start();
        }
    }
#endif
}
