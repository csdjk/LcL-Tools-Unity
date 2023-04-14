using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;

namespace LcLTools
{
    [CustomEditor(typeof(LcLDebugTools))]
    public class LcLDebugToolsEditor : Editor
    {
        public List<string> buttonEventList = new List<string>();
        // 创建一个字典，用来存储方法名和参数列表
        public Dictionary<string, List<string>> buttonEventParams = new Dictionary<string, List<string>>();
        public string[] sceneList;

        private SerializedProperty uiBoxSizeProp;
        private SerializedProperty buttonHeightProp;
        private SerializedProperty fontSizeProp;
        private SerializedProperty lodLevelProp;
        private SerializedProperty postProcessProp;
        private SerializedProperty sceneListProp;
        private SerializedProperty singleListProp;
        private SerializedProperty toggleListProp;
        private SerializedProperty buttonDataListProp;


        private void OnEnable()
        {
            uiBoxSizeProp = serializedObject.FindProperty("uiBoxSize");
            buttonHeightProp = serializedObject.FindProperty("buttonHeight");
            fontSizeProp = serializedObject.FindProperty("fontSize");
            lodLevelProp = serializedObject.FindProperty("lodLevel");
            postProcessProp = serializedObject.FindProperty("postProcess");
            sceneListProp = serializedObject.FindProperty("sceneList");
            singleListProp = serializedObject.FindProperty("singleList");
            toggleListProp = serializedObject.FindProperty("toggleList");
            buttonDataListProp = serializedObject.FindProperty("buttonDataList");
        }

        public override void OnInspectorGUI()
        {
            var debugTools = target as LcLDebugTools;
            UpdateFunctionList();
            UpdateSceneList();
            serializedObject.Update();

            EditorGUILayout.BeginVertical("U2D.createRect");
            {
                EditorGUILayout.PropertyField(uiBoxSizeProp);
                EditorGUILayout.PropertyField(buttonHeightProp);
                EditorGUILayout.PropertyField(fontSizeProp);

            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.PropertyField(lodLevelProp);
            EditorGUILayout.PropertyField(postProcessProp);
            EditorGUILayout.PropertyField(singleListProp, new GUIContent("单选切换："), true);
            EditorGUILayout.PropertyField(toggleListProp, new GUIContent("多选切换："), true);

            DrawButtonDataList();
            DrawSceneList();
        }
        private bool showSceneList = true;

        //Draw SceneList
        private void DrawSceneList()
        {
            showSceneList = EditorGUILayout.BeginFoldoutHeaderGroup(showSceneList, "场景跳转列表：");
            if (showSceneList)
            {
                EditorGUILayout.BeginVertical("box");
                {
                    for (int i = 0; i < sceneListProp.arraySize; i++)
                    {
                        SerializedProperty sceneData = sceneListProp.GetArrayElementAtIndex(i);

                        EditorGUILayout.BeginHorizontal();
                        {
                            SerializedProperty active = sceneData.FindPropertyRelative("active");
                            SerializedProperty name = sceneData.FindPropertyRelative("name");
                            SerializedProperty index = sceneData.FindPropertyRelative("index");

                            active.boolValue = EditorGUILayout.Toggle(active.boolValue, GUILayout.Width(20));

                            var indexValue = Array.IndexOf(sceneList, name.stringValue);
                            int selectedIndex = EditorGUILayout.Popup(Array.IndexOf(sceneList, name.stringValue), sceneList);
                            if (selectedIndex == -1) selectedIndex = 0;
                            name.stringValue = sceneList[selectedIndex];

                            EditorGUILayout.LabelField(new GUIContent(indexValue.ToString(), "场景索引"), GUILayout.Width(50));

                            index.intValue = indexValue;

                            if (GUILayout.Button("-", GUILayout.Width(50)))
                            {
                                sceneListProp.DeleteArrayElementAtIndex(i);
                                i--;
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }

                    if (GUILayout.Button("+"))
                    {
                        sceneListProp.arraySize++;
                        SerializedProperty newSceneData = sceneListProp.GetArrayElementAtIndex(sceneListProp.arraySize - 1);
                        newSceneData.FindPropertyRelative("active").boolValue = true;
                        newSceneData.FindPropertyRelative("name").stringValue = "";
                        newSceneData.FindPropertyRelative("index").intValue = 0;
                    }
                    serializedObject.ApplyModifiedProperties();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

        }

        private bool showButtonDataList = true;

        // Draw button DataListProp
        private void DrawButtonDataList()
        {
            showButtonDataList = EditorGUILayout.BeginFoldoutHeaderGroup(showButtonDataList, "按钮列表：");
            if (showButtonDataList)
            {
                EditorGUILayout.BeginVertical("box");
                {
                    var eventArray = buttonEventList.ToArray();
                    for (int i = 0; i < buttonDataListProp.arraySize; i++)
                    {
                        SerializedProperty buttonData = buttonDataListProp.GetArrayElementAtIndex(i);

                        EditorGUILayout.BeginHorizontal();
                        {
                            SerializedProperty active = buttonData.FindPropertyRelative("active");
                            SerializedProperty name = buttonData.FindPropertyRelative("name");
                            SerializedProperty action = buttonData.FindPropertyRelative("action");

                            active.boolValue = EditorGUILayout.Toggle(active.boolValue, GUILayout.Width(20));
                            EditorGUILayout.PropertyField(name, GUIContent.none);


                            if (action.stringValue == "SwitchKeyword")
                            {
                                SerializedProperty data = buttonData.FindPropertyRelative("data");
                                data.stringValue = EditorGUILayout.TextField(data.stringValue);
                            }

                            int selectedIndex = EditorGUILayout.Popup(Array.IndexOf(eventArray, action.stringValue), eventArray);
                            if (selectedIndex == -1) selectedIndex = 0;
                            action.stringValue = eventArray[selectedIndex];


                            if (GUILayout.Button("-", GUILayout.Width(50)))
                            {
                                buttonDataListProp.DeleteArrayElementAtIndex(i);
                                i--;
                            }

                        }
                        EditorGUILayout.EndHorizontal();
                    }


                    if (GUILayout.Button("+"))
                    {
                        buttonDataListProp.arraySize++;
                        SerializedProperty newButtonData = buttonDataListProp.GetArrayElementAtIndex(buttonDataListProp.arraySize - 1);
                        newButtonData.FindPropertyRelative("active").boolValue = true;
                        newButtonData.FindPropertyRelative("name").stringValue = "";
                        newButtonData.FindPropertyRelative("action").stringValue = eventArray[0];
                    }
                    serializedObject.ApplyModifiedProperties();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

        }
        // 获取所有函数
        private void UpdateFunctionList()
        {
            buttonEventList.Clear();
            buttonEventParams.Clear();
            var debugTools = target as LcLDebugTools;

            var type = debugTools.GetType();
            var methods = type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
            foreach (var item in methods)
            {
                buttonEventList.Add(item.Name);
                buttonEventParams.Add(item.Name, GetMethodParameters(item));
            }
        }

        private List<string> GetMethodParameters(MethodInfo method)
        {
            var parameters = method.GetParameters();
            var param = new List<string>();
            for (int i = 0; i < parameters.Length; i++)
            {
                param.Add(parameters[i].Name);
            }
            return param;
        }

        // get all scene in editor
        private void UpdateSceneList()
        {
            sceneList = EditorBuildSettings.scenes
                          .Where(scene => scene.enabled)
                          .Select(scene => Path.GetFileNameWithoutExtension(scene.path))
                          .ToArray();
        }


        //         public Renderer actorRender;
        //         public Texture2D mainTexture;
        //         public Texture2D texture1;
        //         public float posX = 0f;
        //         public float posY = 0f;
        //         public void MergeTexture()
        //         {
        // #if UNITY_EDITOR
        //             var path = "Assets/LiChangLong/Texture/CombineTexture.png";

        //             var rt = new RenderTexture(mainTexture.width, mainTexture.width, 32);
        //             RenderTexture.active = rt;
        //             Graphics.Blit(mainTexture, rt);
        //             GL.PushMatrix();
        //             GL.LoadPixelMatrix(0, mainTexture.width, mainTexture.width, 0);
        //             Graphics.DrawTexture(new Rect(posX, posY, texture1.width, texture1.height), texture1);

        //             Texture2D png = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
        //             png.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);


        //             png.Apply();
        //             System.IO.File.WriteAllBytes(path, png.EncodeToPNG());
        //             AssetDatabase.ImportAsset(path);
        //             GL.PopMatrix();
        //             RenderTexture.active = null;

        //             // DestroyImmediate(png);
        //             png = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        //             actorRender.sharedMaterial.mainTexture = png;
        // #endif
        //         }

    }
}