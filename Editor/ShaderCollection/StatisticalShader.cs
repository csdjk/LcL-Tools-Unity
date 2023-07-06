using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace LcLTools
{
    public class ShaderVariantCounter : EditorWindow
    {

        private Dictionary<string, int> shaderVariantCounts = new Dictionary<string, int>();
        [MenuItem("Window/Shader Variant Counter")]
        private static void ShowWindow()
        {
            var window = GetWindow<ShaderVariantCounter>();
            window.titleContent = new GUIContent("Shader Variant Counter");
            window.Show();
        }
        private Vector2 scrollPosition = Vector2.zero;
        bool sortReverse = false;
        int shaderCount = 0;
        private string filePath = "F:\\UnityProjects\\Work\\APPGameUnity\\ShaderVariantBuildOutput.txt";
        private void OnGUI()
        {
            // 绘制一个path输入框
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Shader变体Build文件:");
            filePath = GUILayout.TextField(filePath);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            // 居中显示label 
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label($"Shader总数:{shaderVariantCounts.Count},变体总数:{shaderCount}", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            // Create a scrollview container
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            foreach (var kvp in shaderVariantCounts)
            {
                if (GUILayout.Button($"{kvp.Key}: {kvp.Value}"))
                {
                    var shader = Shader.Find(kvp.Key);
                    if (shader != null)
                    {
                        EditorGUIUtility.PingObject(shader);
                    }
                }
            }
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh", GUILayout.Height(50)))
            {
                RefreshData();
            }

            // 排序按钮
            if (GUILayout.Button("Sort", GUILayout.Height(50)))
            {
                sortReverse = !sortReverse;
                if (sortReverse)
                    shaderVariantCounts = shaderVariantCounts.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
                else
                    shaderVariantCounts = shaderVariantCounts.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

            }
            GUILayout.EndHorizontal();

        }

        private void OnEnable()
        {
            RefreshData();
        }




        // Refresh Data Method
        private void RefreshData()
        {
            shaderVariantCounts.Clear();
            var fileContents = File.ReadAllLines(filePath);
            shaderCount = fileContents.Length;

            foreach (var line in fileContents)
            {
                var parts = line.Split(':');
                var shaderName = parts[0].Trim();

                if (!shaderVariantCounts.ContainsKey(shaderName))
                {
                    shaderVariantCounts.Add(shaderName, 0);
                }

                shaderVariantCounts[shaderName]++;
            }
            Repaint();
        }

    }
}