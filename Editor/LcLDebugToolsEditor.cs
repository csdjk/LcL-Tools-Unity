using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;
using Object = System.Object;

namespace LcLTools
{
    [CustomEditor(typeof(LcLDebugTools))]
    public class LcLDebugToolsEditor : Editor
    {
        public List<string> buttonEventList = new List<string>();
        // 创建一个字典，用来存储方法名和参数列表
        public Dictionary<string, List<ParameterInfo>> buttonEventParams = new Dictionary<string, List<ParameterInfo>>();
        public string[] sceneList;

        private SerializedProperty uiBoxSizeProp;
        private SerializedProperty buttonHeightProp;
        private SerializedProperty fontSizeProp;
        private SerializedProperty lodLevelProp;
        private SerializedProperty showLODProp;
        private SerializedProperty sceneListProp;
        private SerializedProperty singleListProp;
        private SerializedProperty toggleListProp;
        private SerializedProperty buttonDataListProp;
        private SerializedProperty highConsumptionProp;
        private SerializedProperty highIterationsProp;
        private SerializedProperty showParamWindowProp;
        private SerializedProperty paramObjectsProp;

        private void OnEnable()
        {

            uiBoxSizeProp = serializedObject.FindProperty("uiBoxSize");
            buttonHeightProp = serializedObject.FindProperty("buttonHeight");
            fontSizeProp = serializedObject.FindProperty("fontSize");
            lodLevelProp = serializedObject.FindProperty("lodLevel");
            showLODProp = serializedObject.FindProperty("showLOD");
            sceneListProp = serializedObject.FindProperty("sceneList");
            singleListProp = serializedObject.FindProperty("singleList");
            toggleListProp = serializedObject.FindProperty("toggleList");
            buttonDataListProp = serializedObject.FindProperty("buttonDataList");
            highConsumptionProp = serializedObject.FindProperty("highConsumption");
            highIterationsProp = serializedObject.FindProperty("highIterations");

            showParamWindowProp = serializedObject.FindProperty("showParamWindow");
            paramObjectsProp = serializedObject.FindProperty("paramObjects");

        }

        public override void OnInspectorGUI()
        {
            var debugTools = target as LcLDebugTools;
            UpdateFunctionList();
            UpdateSceneList();
            serializedObject.Update();
            EditorGUILayout.BeginHorizontal();
            {
                // check change 
                EditorGUI.BeginChangeCheck();
                {
                    EditorGUILayout.PropertyField(highConsumptionProp);
                    EditorGUILayout.PropertyField(highIterationsProp);
                }
                if (EditorGUI.EndChangeCheck())
                {
                    // if (highConsumptionProp.boolValue)
                    // {
                    //     HeavyPostProcessingFeature.Enable(highIterationsProp.intValue);
                    // }
                    // else
                    // {
                    //     HeavyPostProcessingFeature.Disable();
                    // }
                }
            }
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.PropertyField(uiBoxSizeProp);
            EditorGUILayout.PropertyField(buttonHeightProp);
            EditorGUILayout.PropertyField(fontSizeProp);

            EditorGUILayout.BeginVertical("U2D.createRect");
            {
                EditorGUILayout.LabelField("Button Event");
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal();
            {
                showLODProp.boolValue = EditorGUILayout.Toggle(showLODProp.boolValue);
                lodLevelProp.enumValueIndex = EditorGUILayout.Popup(lodLevelProp.enumValueIndex, lodLevelProp.enumDisplayNames);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(singleListProp, new GUIContent("单选切换："), true);
            EditorGUILayout.PropertyField(toggleListProp, new GUIContent("多选切换："), true);

            DrawButtonDataList();
            DrawSceneList();

            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical("U2D.createRect");
            {
                EditorGUILayout.PropertyField(showParamWindowProp, new GUIContent("显示参数调节面板"), true);
            }
            EditorGUILayout.EndVertical();
            DrawParamsList();

            serializedObject.ApplyModifiedProperties();
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
        // 拖拽处理
        void HandleDragAndDrop(Action<UnityEngine.Object[]> callback)
        {
            Event e = Event.current;
            Rect drop_area = GUILayoutUtility.GetLastRect();
            if ((e.type == EventType.DragUpdated || e.type == EventType.DragPerform) && drop_area.Contains(e.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                // 用户已经放下了被拖动的对象
                if (e.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    callback?.Invoke(DragAndDrop.objectReferences);
                }

                Event.current.Use();
            }
        }

        private bool showButtonDataList = true;
        private int selectedIndex = 0;

        // Draw button DataListProp
        private void DrawButtonDataList()
        {
            showButtonDataList = EditorGUILayout.BeginFoldoutHeaderGroup(showButtonDataList, "按钮列表：");
            if (showButtonDataList)
            {
                EditorGUILayout.BeginVertical("box");
                {
                    var eventArray = buttonEventList.ToArray();
                    // var paramsArray = buttonEventParams.ToArray();

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

                            // 绘制参数列表
                            SerializedProperty paramListProperty = buttonData.FindPropertyRelative("paramList");
                            if (buttonEventParams.TryGetValue(action.stringValue, out var paramList))
                            {
                                paramListProperty.arraySize = paramList.Count;

                                for (int j = 0; j < paramList.Count; j++)
                                {
                                    var paramData = paramList[j];
                                    var param = paramListProperty.GetArrayElementAtIndex(j);
                                    var intValue = param.FindPropertyRelative("intValue");
                                    var floatValue = param.FindPropertyRelative("floatValue");
                                    var stringValue = param.FindPropertyRelative("stringValue");
                                    var type = param.FindPropertyRelative("type");

                                    if (paramData.ParameterType == typeof(string))
                                    {
                                        type.enumValueIndex = (int)ParamType.String;
                                        GUI.SetNextControlName("goPath");
                                        stringValue.stringValue = EditorGUILayout.TextField(stringValue?.stringValue != null ? stringValue.stringValue : paramData.Name);

                                        HandleDragAndDrop(draggedObjects =>
                                        {
                                            GameObject go = draggedObjects[0] as GameObject;
                                            stringValue.stringValue = go?.transform.GetPath();
                                        });
                                    }
                                    else if (paramData.ParameterType == typeof(int))
                                    {
                                        type.enumValueIndex = (int)ParamType.Int;
                                        intValue.intValue = EditorGUILayout.IntField(intValue.intValue);
                                    }
                                    else if (paramData.ParameterType == typeof(float))
                                    {
                                        type.enumValueIndex = (int)ParamType.Float;
                                        floatValue.floatValue = EditorGUILayout.FloatField(floatValue.floatValue);
                                    }
                                    // else if (paramData.ParameterType.IsSubclassOf(typeof(MonoBehaviour)) || paramData.ParameterType.Equals(typeof(GameObject)) || paramData.ParameterType.IsSubclassOf(typeof(ScriptableObject)))
                                    // {
                                    //     if (value == null || value is "")
                                    //     {
                                    //         value = EditorGUILayout.ObjectField(null, paramData.ParameterType, true);
                                    //     }
                                    //     else
                                    //     {
                                    //         value = EditorGUILayout.ObjectField((UnityEngine.Object)value, paramData.ParameterType, true);
                                    //     }
                                    // }
                                    // param.SetValue(value);
                                    // set dirt flag
                                    // if (GUI.changed)
                                    // {
                                    //     EditorUtility.SetDirty(target);
                                    // }
                                }
                            }
                            // 绘制方法列表
                            selectedIndex = EditorGUILayout.Popup(Array.IndexOf(eventArray, action.stringValue), eventArray);
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
                        ParamData newParamData = new ParamData
                        {
                            // intValue = 0,
                            // floatValue = 0.0f,
                            // stringValue = ""
                        };
                        newButtonData.FindPropertyRelative("paramList").arraySize = 1;
                        newButtonData.FindPropertyRelative("paramList").GetArrayElementAtIndex(0).managedReferenceValue = newParamData;

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
                buttonEventParams.TryAdd(item.Name, GetMethodParameters(item));
            }
        }

        private List<ParameterInfo> GetMethodParameters(MethodInfo method)
        {
            var parameters = method.GetParameters();
            var param = new List<ParameterInfo>();
            for (int i = 0; i < parameters.Length; i++)
            {
                param.Add(parameters[i]);
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


        private bool showParamList = true;

        private void DrawParamsList()
        {
            showParamList = EditorGUILayout.BeginFoldoutHeaderGroup(showParamList, "参数列表");
            if (showParamList)
            {
                EditorGUILayout.BeginVertical("box");
                {
                    for (int i = 0; i < paramObjectsProp.arraySize; i++)
                    {
                        SerializedProperty paramData = paramObjectsProp.GetArrayElementAtIndex(i);

                        EditorGUILayout.BeginHorizontal();
                        {
                            SerializedProperty active = paramData.FindPropertyRelative("active");
                            SerializedProperty script = paramData.FindPropertyRelative("script");
                            SerializedProperty paramName = paramData.FindPropertyRelative("paramName");

                            active.boolValue = EditorGUILayout.Toggle(active.boolValue, GUILayout.Width(20));

                            var paramList = GetFieldList(script.objectReferenceValue?.GetType());

                            var indexValue = Array.IndexOf(paramList, paramName.stringValue);
                            int selectedIndex = EditorGUILayout.Popup(indexValue, paramList);
                            if (selectedIndex == -1) selectedIndex = 0;
                            paramName.stringValue = paramList[selectedIndex];

                            script.objectReferenceValue = EditorGUILayout.ObjectField(script.objectReferenceValue, typeof(MonoBehaviour), true);



                            if (GUILayout.Button("-", GUILayout.Width(50)))
                            {
                                paramObjectsProp.DeleteArrayElementAtIndex(i);
                                i--;
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }

                    if (GUILayout.Button("+"))
                    {
                        paramObjectsProp.arraySize++;
                        SerializedProperty newParamData = paramObjectsProp.GetArrayElementAtIndex(paramObjectsProp.arraySize - 1);
                        newParamData.FindPropertyRelative("active").boolValue = true;
                        newParamData.FindPropertyRelative("paramName").stringValue = "";
                        newParamData.FindPropertyRelative("script").objectReferenceValue = null;
                    }
                    serializedObject.ApplyModifiedProperties();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

        }

        List<string> fieldList = new List<string>();
        private string[] GetFieldList(Type type)
        {
            if (type == null) return new string[1];
            fieldList.Clear();
            var fields = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
            if (fields.Length == 0)
            {
                fieldList.Add("None");
                return fieldList.ToArray();
            }
            foreach (var item in fields)
            {
                fieldList.Add(item.Name);
            }
            return fieldList.ToArray();
        }
    }
}