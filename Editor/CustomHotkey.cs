using System.IO;
/*
 * @Descripttion: 自定义快捷键
 * @Author: lichanglong
 * @Date: 2020-12-18 11:33:56
 */

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace LcLTools
{
    /// <summary>
    /// 自定义快捷键
    /// </summary>
    public class CustomHotkey
    {
        [MenuItem("LcLTools/HotKeys/播放 _F11")]
        static void EditorPlayCommand()
        {
            EditorApplication.isPlaying = !EditorApplication.isPlaying;
        }

        [MenuItem("LcLTools/HotKeys/暂停 _F12")]
        static void EditorPauseCommand()
        {
            EditorApplication.isPaused = !EditorApplication.isPaused;
        }


        // ================================ 【Alt + ...】 ================================
        /// <summary>
        /// 定位 material、shader
        /// </summary>
        static double clickTime;
        static double doubleClickTime = 0.3;
        static List<Renderer> renders = new List<Renderer>();
        [MenuItem("LcLTools/HotKeys/定位到Shader &s")]
        static void QuickPositioningShader()
        {
            var selectObject = Selection.objects[0];
            if (selectObject == null) return;
            var go = selectObject as GameObject;
            if (go)
            {
                renders.Clear();
                go.GetComponentsInChildren<Renderer>(renders);
                if (renders.Count == 0) return;

                var mat = renders[0].sharedMaterial;
                if (mat == null) return;
                // 双击选中shader
                if ((EditorApplication.timeSinceStartup - clickTime) < doubleClickTime)
                {
                    EditorGUIUtility.PingObject(mat.shader);
                    return;
                }
                EditorGUIUtility.PingObject(mat);
                clickTime = EditorApplication.timeSinceStartup;
            }
            else
            {
                var mat = selectObject as Material;
                EditorGUIUtility.PingObject(mat?.shader);
            }
        }

        [MenuItem("LcLTools/HotKeys/创建Material &c")]
        static void CreateMaterial()
        {
            var objects = Selection.objects;
            if (objects == null) return;
            foreach (var obj in objects)
            {
                var shader = obj as Shader;

                var path = AssetDatabase.GetAssetPath(obj);
                if (shader)
                {
                    Material material = new Material(shader);
                    path = path.Replace(".shader", ".mat");
                    AssetDatabase.CreateAsset(material, path);
                }
                else
                {
                    Material material;
                    // 判断是否URP
                    if (GraphicsSettings.renderPipelineAsset != null && GraphicsSettings.renderPipelineAsset.GetType().Name == "UniversalRenderPipelineAsset")
                        material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    else
                        material = new Material(Shader.Find("Standard"));
                        
                    if (!AssetDatabase.IsValidFolder(path))
                    {
                        path = Path.GetDirectoryName(path);
                    }
                    path = Path.Combine(path, "Material.mat");
                    path = AssetDatabase.GenerateUniqueAssetPath(path);
                    AssetDatabase.CreateAsset(material, path);
                }
            }
        }


        // 定位到当前使用的RenderAssets 
        [MenuItem("LcLTools/HotKeys/定位到RenderAssets &r")]
        static void QuickPositioningRenderAssets()
        {
            var pipelineAsset = QualitySettings.GetRenderPipelineAssetAt(QualitySettings.GetQualityLevel());
            EditorGUIUtility.PingObject(pipelineAsset);
        }


        [MenuItem("LcLTools/HotKeys/Project Settings &1")]
        static void OpenProjectSettings()
        {
            // 打开Project Settings窗口
            EditorApplication.ExecuteMenuItem("Edit/Project Settings...");
        }
        // ---------------------【Ctrl + ...】--------------------------
        [MenuItem("LcLTools/HotKeys/快速定位到Model %m")]
        static void QuickPositioningModel()
        {
            var assetObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/Models");
            EditorGUIUtility.PingObject(assetObj);
        }




        // ---------------------【Shift + ...】--------------------------

        [MenuItem("LcLTools/HotKeys/重置Position #r")]
        static void QuickResetPosition()
        {
            var trsArr = Selection.transforms;
            if (trsArr == null) return;
            foreach (var trs in trsArr)
            {
                Undo.RecordObject(trs, "Reset Position");
                trs.localPosition = Vector3.zero;
            }
        }

        [MenuItem("LcLTools/HotKeys/隐藏显示Object #a")]
        static void QuickSetActive()
        {
            // var go = Selection.activeGameObject;

            var trsArr = Selection.transforms;
            if (trsArr == null) return;
            foreach (var trs in trsArr)
            {
                Undo.RecordObject(trs.gameObject, "Show Objects");
                trs.gameObject.SetActive(!trs.gameObject.activeSelf);
            }
        }

        static MethodInfo clearMethod = null;
        [MenuItem("LcLTools/HotKeys/清空日志 #c")]
        public static void ClearConsole()
        {
            if (clearMethod == null)
            {
                Type log = typeof(EditorWindow).Assembly.GetType("UnityEditor.LogEntries");
                clearMethod = log.GetMethod("Clear");
            }
            clearMethod.Invoke(null, null);
        }



        // ---------------------【Hierarchy】--------------------------

        // 实例化自定义模型
        static void CreateCustomModel(string path)
        {
            var assetObj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            GameObject go = PrefabUtility.InstantiatePrefab(assetObj) as GameObject;
            PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

            var selectGo = Selection.activeGameObject;
            int index = 0;
            if (selectGo && go)
            {
                go.transform.parent = selectGo.transform;
                index = selectGo.transform.childCount;
            }
            else
            {
                var m_Scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                var list = m_Scene.GetRootGameObjects();
                index = list.Length;
            }

            go.transform.SetSiblingIndex(index);
            Undo.RegisterCreatedObjectUndo(go, "CreateCustomModel");
        }

        [MenuItem("GameObject/CreateCustomModel/Jan", false, 0)]
        static void CreateJan()
        {
            CreateCustomModel("Assets/Models/Jan/Jan_Fight_Idle.prefab");
        }
        [MenuItem("GameObject/CreateCustomModel/2B", false, 0)]
        static void Create2B()
        {
            CreateCustomModel("Assets/Models/NieRAutomata/2B_Run/2b_run.fbx");
        }
        [MenuItem("GameObject/CreateCustomModel/Omega", false, 0)]
        static void CreateFortniteOmega()
        {
            CreateCustomModel("Assets/Models/FortniteOmega/Omega.prefab");
        }
        [MenuItem("GameObject/CreateCustomModel/RobotKyle", false, 0)]
        static void CreateRobotKyle()
        {
            CreateCustomModel("Assets/Models/RobotKyle/RobotKyle.fbx");
        }

        [MenuItem("GameObject/CreateCustomModel/Other/BunnyLow", false, 0)]
        static void CreateBunnyLow()
        {
            CreateCustomModel("Assets/Models/Other/BunnyLow.obj");
        }
        [MenuItem("GameObject/CreateCustomModel/Other/Dragon", false, 0)]
        static void CreateDragon()
        {
            CreateCustomModel("Assets/Models/Other/Dragon.obj");
        }

        [MenuItem("GameObject/CreateCustomModel/Other/Knot", false, 0)]
        static void CreateKnot()
        {
            CreateCustomModel("Assets/Models/Other/Knot.FBX");
        }
        [MenuItem("GameObject/CreateCustomModel/Other/Teapot", false, 0)]
        static void CreateTeapot()
        {
            CreateCustomModel("Assets/Models/Other/Teapot.FBX");
        }
        [MenuItem("GameObject/CreateCustomModel/Other/Sphere1", false, 0)]
        static void CreateSphere1()
        {
            CreateCustomModel("Assets/Models/Other/sphere1.obj");
        }
        [MenuItem("GameObject/CreateCustomModel/Other/Sphere2", false, 0)]
        static void CreateSphere2()
        {
            CreateCustomModel("Assets/Models/Other/sphere2.FBX");
        }
        // end

    }

}
