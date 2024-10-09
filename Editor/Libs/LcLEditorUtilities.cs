using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LcLTools
{
    public class LcLEditorUtilities
    {
        public static StyleSheet GetStyleSheet(string name)
        {
            return GetAssetByName<StyleSheet>(name);
        }

        public static T GetAssetByName<T>(string name) where T : UnityEngine.Object
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) == name)
                    return AssetDatabase.LoadAssetAtPath<T>(path);
            }

            return null;
        }

        /// <summary>
        /// 获取所有runtime的目录
        /// </summary>
        /// <returns></returns>
        public static List<string> GetAllRuntimeDirects()
        {
            //搜索所有资源
            var root = Application.dataPath;
            //获取根路径所有runtime
            var directories = Directory.GetDirectories(root, "*", SearchOption.TopDirectoryOnly).ToList();

            //ret
            List<string> retList = new List<string>();
            foreach (var dirt in directories)
            {
                //
                var _dirt = dirt + "/Runtime";
                if (Directory.Exists(_dirt))
                {
                    _dirt = _dirt.Replace("\\", "/").Replace(Application.dataPath, "Assets");
                    retList.Add(_dirt);
                }
            }

            return retList;
        }

        /// <summary>
        /// 资产转GUID
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        static public string AssetPathToGUID(string assetPath)
        {
            if (assetPath.EndsWith("/"))
            {
                assetPath = assetPath.Remove(assetPath.Length - 1, 1);
            }

            if (File.Exists(assetPath) || Directory.Exists(assetPath))
            {
                return AssetDatabase.AssetPathToGUID(assetPath);
            }

            Debug.LogError("资产不存在:" + assetPath);

            return null;
        }

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

        // 保存RenderTexture
        public static void SaveRenderTextureToTexture(RenderTexture rt, string path,
            TextureFormat format = TextureFormat.RGB24)
        {
            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(rt.width, rt.height, format, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            RenderTexture.active = null;

            var ext = Path.GetExtension(path);
            byte[] bytes;
            switch (ext)
            {
                case ".png":
                    bytes = tex.EncodeToPNG();
                    break;
                case ".tga":
                    bytes = tex.EncodeToTGA();
                    break;
                default:
                    bytes = tex.EncodeToJPG();
                    break;
            }

            File.WriteAllBytes(path, bytes);
            AssetDatabase.ImportAsset(path);
            Debug.Log("Saved to " + path);
        }

        /// <summary>
        /// 加载纹理,相对路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Texture2D LoadTexture(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        // path :
        /// <summary>
        /// 加载纹理,绝对路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Texture2D LoadTextureAbsolutePath(string path)
        {
            Texture2D tex = null;
            byte[] fileData;
            string filePath = path;
            if (File.Exists(filePath))
            {
                fileData = File.ReadAllBytes(filePath);
                tex = new Texture2D(1024, 1024);
                tex.LoadImage(fileData);
                return tex;
            }

            return null;
        }

        /// <summary>
        /// 根据预设名称获取绝对路径
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        public static string GetAssetAbsolutePath(UnityEngine.Object go)
        {
            if (go == null) return "";
            string str = Application.dataPath.Replace("Assets", "");
            string path = AssetDatabase.GetAssetPath(go);
            string dir_path = System.IO.Path.GetFullPath(str + path);
            return dir_path;
        }

        /// <summary>
        /// 获取当前选中的第一个物体路径
        /// </summary>
        public static string GetSelectionAssetPath()
        {
            return GetAssetAbsolutePath(Selection.objects[0]);
        }

        /// <summary>
        /// 获取当前选择的所有物体路径
        /// </summary>
        public static List<string> GetSelectionAssetPaths()
        {
            if (Selection.objects.Length == 0)
            {
                return null;
            }

            List<string> paths = new List<string>();
            foreach (var item in Selection.objects)
            {
                paths.Add(GetAssetAbsolutePath(item));
            }

            return paths;
        }


        public static string GetAssetDirectory(UnityEngine.Object asset)
        {
            if (asset == null)
            {
                Debug.LogError("Asset is null");
                return null;
            }

            string assetPath = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogError("Asset path is invalid");
                return null;
            }

            return Path.GetDirectoryName(assetPath);
        }

        /// <summary>
        /// 在传入的资源同目录下创建资源目录，并返回新资源的路径。
        /// </summary>
        /// <param name="asset">资源对象。</param>
        /// <param name="newAssetName">新资源的名称。</param>
        /// <param name="createNewFolder">是否创建新文件夹。</param>
        /// <param name="newFolderName">新文件夹的名称（可选，默认为"NewFolder"）。</param>
        /// <returns>新资源的路径，如果资源对象为空或路径无效，则返回null。</returns>
        public static string CreatePathInAssetDirectory(UnityEngine.Object asset, string newAssetName,
            bool createNewFolder = false, string newFolderName = "NewFolder")
        {
            if (asset == null)
            {
                Debug.LogError("Asset is null");
                return null;
            }
            string assetPath = AssetDatabase.GetAssetPath(asset);
            return CreatePathInAssetDirectory(assetPath, newAssetName, createNewFolder, newFolderName);
        }

        public static string CreatePathInAssetDirectory(string path, string newAssetName,
            bool createNewFolder = false, string newFolderName = "NewFolder")
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("path is invalid");
                return null;
            }

            string directoryPath = Path.GetDirectoryName(path);
            if (createNewFolder)
            {
                string newFolderPath = Path.Combine(directoryPath, newFolderName);
                if (!AssetDatabase.IsValidFolder(newFolderPath))
                {
                    AssetDatabase.CreateFolder(directoryPath, newFolderName);
                }

                return Path.Combine(newFolderPath, newAssetName);
            }

            return Path.Combine(directoryPath, newAssetName);
        }


        /// <summary>
        /// 通过开始菜单查找应用程序的路径
        /// </summary>
        /// <param name="appName">要查找的应用程序名称</param>
        /// <param name="appPath">应用程序路径</param>
        /// <returns></returns>
        public static bool GetApplicationByStartMenu(string appName, out string appPath)
        {
            appPath = null;
            /// %AppData%\Microsoft\Windows\Start Menu\Programs
            string startMenuPath =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs");
            /// %ProgramData%\Microsoft\Windows\Start Menu\Programs
            string allStartMenuPath = Path.Combine(Environment.GetEnvironmentVariable("ALLUSERSPROFILE"),
                "Microsoft\\Windows\\Start Menu\\Programs");

            var appList = FileSystem.GetAllFilePath(startMenuPath, "*.lnk");
            appList.AddRange(FileSystem.GetAllFilePath(allStartMenuPath, "*.lnk"));

            appName = appName.ToLower();
            foreach (var path in appList)
            {
                if (Path.GetFileNameWithoutExtension(path).ToLower().Contains(appName))
                {
                    appPath = path;
                    return true;
                }
            }

            return false;
        }
    }
}
