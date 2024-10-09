using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_PIPELINE_URP
using UnityEngine.Rendering.Universal;
#endif

using UnityEngine.SceneManagement;
using UnityEngine.Experimental.Rendering;
using System.Reflection;
using Object = UnityEngine.Object;
using UnityEngine.Profiling;

namespace LcLTools
{
    public enum LodLevel
    {
        LOD100 = 100,
        LOD200 = 200,
        LOD300 = 300,
    }
    public enum ParamType
    {
        Int,
        Float,
        String,
    }
    [Serializable]
    public class ParamData
    {
        public int intValue;
        public float floatValue;
        public string stringValue;
        public ParamType type;
    }
    [Serializable]
    public class ButtonData
    {
        public bool active;
        public string name;
        public string action;
        public bool buttonState;
        [SerializeReference]
        public List<ParamData> paramList = new List<ParamData>();
    }
    [Serializable]
    public class SliderData
    {
        public bool active;
        public string name;
        public string action;
        public float value;
    }

    [Serializable]
    public struct SceneData
    {
        public bool active;
        public string name;
        public int index;
    }

    [Serializable]
    public struct ParamObjectData
    {
        public bool active;
        public MonoBehaviour script;
        public string paramName;
    }
    [ExecuteAlways, AddComponentMenu("LcLTools/LcLDebugTools", 0)]
    public class LcLDebugTools : MonoBehaviour
    {
        //---------------------------GUI-------------------------------------
        static int windowID = 101;
        public Vector2 uiBoxSize = new Vector2(600, 250);
        [Range(10, 200)]
        public float buttonHeight = 50;
        [Range(10, 100)]
        public int fontSize = 25;
        private Rect uiBoxRect = new Rect(0, 0, 0, 0);
        //---------------------------Param Window-------------------------------------

        static int paramWindowID = 102;
        private Rect paramBoxRect = new Rect(0, 0, 0, 0);
        public bool showParamWindow = false;
        public MonoBehaviour[] paramGoList;
        public List<ParamObjectData> paramObjects;


        //---------------------------GUI-------------------------------------

        public bool showLOD = true;
        public LodLevel lodLevel = LodLevel.LOD300;

        public List<GameObject> gameObjectList = new List<GameObject>();

        public List<SceneData> sceneList;
        public GameObject[] singleList;
        public GameObject[] toggleList;

        [Header("按钮列表")]
        [SerializeField, HideInInspector]
        private List<ButtonData> buttonDataList;
        [SerializeField, HideInInspector]

        private float renderScale = 1;

        private GUIStyle enableStyle;
        private GUIStyle disableStyle;
        private void Awake()
        {
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        [SerializeField] private int highIterations = 200;
        [SerializeField] private bool highConsumption = false;
        void OnEnable()
        {
            // if (highConsumption)
            // {
            //     HeavyPostProcessingFeature.Enable(highIterations);
            // }
        }

        void OnDisable()
        {
            // HeavyPostProcessingFeature.Disable();
        }

        // public GameObject GetGameObject(string name)
        // {
        //     var go = gameObjectList.Find((go) => go.name == name);
        //     go = go ? go : GameObject.Find(name);
        //     return go;
        // }
        // public GameObject GetGameObject(int index)
        // {
        //     return gameObjectList[index];
        // }
        // public T GetGameObject<T>() where T : Component
        // {
        //     return gameObjectList.Find((go) => go.TryGetComponent(out T t)).GetComponent<T>();
        // }
        // public List<T> GetGameObjects<T>() where T : Component
        // {
        //     return gameObjectList.FindAll((go) => go.TryGetComponent(out T t)).Select((go) => go.GetComponent<T>()).ToList();
        // }

        public GUIStyle GetStyle(bool value)
        {
            return value ? enableStyle : disableStyle;
        }
        public bool Button(string name, bool active = false)
        {
            return GUILayout.Button(name, GetStyle(active), GUILayout.Height(buttonHeight));
        }


        public void SRPSwitch()
        {
            GraphicsSettings.useScriptableRenderPipelineBatching = !GraphicsSettings.useScriptableRenderPipelineBatching;
        }



        public void GotoScene(int index)
        {
            SceneManager.LoadScene(index);
        }

        public void SingleGoSwitch(GameObject value)
        {
            foreach (var item in singleList)
            {
                item.SetActive(item == value);
            }
        }

        public void HideSingleList()
        {
            foreach (var item in singleList)
            {
                item.SetActive(false);
            }
        }

        public void ToggleListSwitch(GameObject go)
        {
            go.SetActive(!go.activeSelf);
        }
        public void HideToggleList()
        {
            foreach (var item in toggleList)
            {
                item.SetActive(false);
            }
        }

        public bool SwitchKeyword(string keyword)
        {
            var enable = Shader.IsKeywordEnabled(keyword);
            SetKeyword(!enable, keyword);
            return !enable;
        }

        public void SetKeyword(bool value, string keyword)
        {
            if (value)
            {
                Shader.EnableKeyword(keyword);
            }
            else
            {
                Shader.DisableKeyword(keyword);
            }
        }

        private Vector2Int screen = new Vector2Int(Screen.width, Screen.height);
        public void ChangeResolution(int v)
        {
            switch (v)
            {
                case 0:
                    Screen.SetResolution(screen.x, screen.y, true);
                    break;
                case 1:
                    Screen.SetResolution(screen.x / 2, screen.y / 2, true);
                    break;
            }
        }

        public string GetSRPState()
        {
            return GraphicsSettings.useScriptableRenderPipelineBatching ? "SRP(ing...)" : "SRP";
        }


        private void HighConsumption()
        {
            if (Application.isPlaying && highConsumption)
            {
                Profiler.BeginSample("HighConsumption");
                for (int i = 0; i < highIterations; i++)
                {
                    float result = Mathf.Sqrt(i);
                    result *= Mathf.Sin(result);
                    result /= Mathf.Cos(result);
                    result += Mathf.Tan(result);
                }
                Profiler.BeginSample(name);
            }
        }

        void OnValidate()
        {
            Shader.globalMaximumLOD = (int)lodLevel;
        }


        private int prevScreenWidth;
        private int prevScreenHeight;
        bool isInit = true;
        void OnGUI()
        {
            // HighConsumption();
            if (uiBoxRect == null || paramBoxRect == null)
            {
                return;
            }
            if (isInit)
            {
                isInit = false;

                enableStyle = new GUIStyle(GUI.skin.button);
                disableStyle = new GUIStyle(GUI.skin.button);
                enableStyle.normal.textColor = Color.green;
                enableStyle.hover.textColor = Color.green;
                enableStyle.fontSize = fontSize;
                disableStyle.normal.textColor = Color.white;
                disableStyle.fontSize = fontSize;
                uiBoxRect.x = Screen.width - uiBoxSize.x;
                uiBoxRect.y = Screen.height - uiBoxSize.y;
                // pos is right top
                paramBoxRect.x = Screen.width - uiBoxSize.x;
                paramBoxRect.y = 0;
            }
            // 判断屏幕分辨率是否发生变化
            if (prevScreenWidth != Screen.width || prevScreenHeight != Screen.height)
            {
                prevScreenWidth = Screen.width;
                prevScreenHeight = Screen.height;
                uiBoxRect.x = Screen.width - uiBoxSize.x;
                uiBoxRect.y = Screen.height - uiBoxSize.y;
            }
            uiBoxRect = GUI.Window(windowID, uiBoxRect, WindowCallBack, "Button");
            uiBoxRect.width = uiBoxSize.x;
            uiBoxRect.height = uiBoxSize.y;


            if (showParamWindow)
            {
                paramBoxRect = GUI.Window(paramWindowID, paramBoxRect, ParamWindowCallBack, "Params");
                paramBoxRect.width = uiBoxSize.x;
                paramBoxRect.height = uiBoxSize.y;
            }

        }
        private void ParamWindowCallBack(int windowID)
        {
            var width = GUILayout.Width(100);
            var height = GUILayout.Height(30);
            GUI.skin.button.fontSize = fontSize;
            GUI.skin.label.fontSize = fontSize;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.backgroundColor = new Color(0, 0, 0, 0.5f);
            if (highConsumption)
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("计算量", height, GUILayout.Width(150));
                    highIterations = (int)GUILayout.HorizontalSlider(highIterations, 0, 1000, GUILayout.Height(30));
                    highIterations = int.Parse(GUILayout.TextField(highIterations.ToString(), enableStyle, GUILayout.Height(30), GUILayout.Width(100)));
                }
                GUILayout.EndHorizontal();
            }
#if UNITY_PIPELINE_URP
            // GUILayout.Label($"ReversedZ: {SystemInfo.usesReversedZBuffer}", GetStyle(SystemInfo.usesReversedZBuffer));
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("RenderScale", height, GUILayout.Width(150));
                var renderScaleTemp = renderScale;
                renderScale = GUILayout.HorizontalSlider(renderScale, 0, 1.2f, GUILayout.Height(30));
                renderScale = (float)Math.Round(renderScale, 2);
                renderScale = float.Parse(GUILayout.TextField(renderScale.ToString(), enableStyle, GUILayout.Height(30), GUILayout.Width(100)));
                // set render scale
                if (renderScaleTemp != renderScale)
                {
                    var urpAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
                    urpAsset.renderScale = renderScale;
                }
            }
            GUILayout.EndHorizontal();
#endif

            GUILayout.BeginVertical();
            {

                foreach (var data in paramObjects)
                {
                    if (data.script == null || !data.active)
                    {
                        continue;
                    }
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label(data.paramName, GUILayout.Height(30), GUILayout.Width(150));
                        FieldInfo field = data.script.GetType().GetField(data.paramName);
                        if (field != null)
                        {
                            // 获取RangeAttribute
                            RangeAttribute rangeAttribute = field.GetCustomAttribute<RangeAttribute>();

                            // 如果字段有RangeAttribute，绘制Slider类型
                            if (rangeAttribute != null)
                            {
                                if (field.FieldType == typeof(float))
                                {
                                    var value = (float)field.GetValue(data.script);
                                    value = GUILayout.HorizontalSlider(value, rangeAttribute.min, rangeAttribute.max, height);
                                    value = (float)Math.Round(value, 2);
                                    value = float.Parse(GUILayout.TextField(value.ToString(), enableStyle, height, width));
                                    field.SetValue(data.script, value);
                                }
                                else if (field.FieldType == typeof(int))
                                {
                                    var value = (int)field.GetValue(data.script);
                                    value = (int)GUILayout.HorizontalSlider(value, rangeAttribute.min, rangeAttribute.max, height);
                                    value = int.Parse(GUILayout.TextField(value.ToString(), enableStyle, height, width));
                                    field.SetValue(data.script, value);
                                }
                            }
                            else
                            {
                                if (field.FieldType == typeof(float))
                                {
                                    var value = (float)field.GetValue(data.script);
                                    value = (float)Math.Round(value, 2);
                                    value = float.Parse(GUILayout.TextField(value.ToString(), enableStyle, height, width));
                                    field.SetValue(data.script, value);
                                }
                                else if (field.FieldType == typeof(int))
                                {
                                    var value = (int)field.GetValue(data.script);
                                    value = int.Parse(GUILayout.TextField(value.ToString(), enableStyle, height, width));
                                    field.SetValue(data.script, value);
                                }
                                else if (field.FieldType == typeof(bool))
                                {
                                    var value = (bool)field.GetValue(data.script);
                                    value = GUILayout.Toggle(value, "", height, width);
                                    field.SetValue(data.script, value);
                                }
                            }
                        }
                    }
                    GUILayout.EndHorizontal();
                }

            }
            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, Screen.width, Screen.height));
        }
        private void WindowCallBack(int windowID)
        {
            GUI.skin.button.fontSize = fontSize;
            GUI.skin.label.fontSize = fontSize;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.backgroundColor = new Color(0, 0, 0, 0.5f);
            GUILayout.BeginHorizontal();
            {

                // 单选开关切换
                if (singleList != null && singleList.Length > 0)
                {
                    GUILayout.BeginVertical();
                    {
                        Button("单选开关");
                        foreach (var item in singleList)
                        {
                            if (item)
                            {
                                if (Button(item.name, item.activeSelf))
                                {
                                    SingleGoSwitch(item);
                                }
                            }
                        }
                        if (Button("Hide All", false))
                        {
                            HideSingleList();
                        }
                    }
                    GUILayout.EndVertical();
                }
                // 多选开关
                if (toggleList != null && toggleList.Length > 0)
                {
                    GUILayout.BeginVertical();
                    {
                        Button("多选开关");
                        foreach (var item in toggleList)
                        {
                            if (item)
                            {
                                if (Button(item.name, item.activeSelf))
                                {
                                    ToggleListSwitch(item);
                                }
                            }
                        }
                        if (Button("Hide All", false))
                        {
                            HideToggleList();
                        }
                    }
                    GUILayout.EndVertical();
                }

                // Scene List
                if (sceneList != null && sceneList.Count > 0)
                {
                    GUILayout.BeginVertical();
                    {
                        Button("Scene List");
                        foreach (var item in sceneList)
                        {
                            if (item.active && Button(item.name, item.active))
                            {
                                GotoScene(item.index);
                            }
                        }
                    }
                    GUILayout.EndVertical();
                }
                // Draw buttonDataList
                if (buttonDataList != null)
                {
                    GUILayout.BeginVertical();
                    {
                        for (int i = 0; i < buttonDataList.Count; i++)
                        {
                            var item = buttonDataList[i];
                            if (item.active && Button(item.name, item.buttonState))
                            {
                                item.buttonState = CallFunction(item);
                            }
                        }
                    }
                    GUILayout.EndVertical();
                }

                if (showLOD)
                {
                    GUILayout.BeginVertical();
                    {
                        GUILayout.Space(10);
                        foreach (LodLevel lod in Enum.GetValues(typeof(LodLevel)))
                        {
                            if (Button(lod.ToString(), Shader.globalMaximumLOD == (int)lod))
                            {
                                Shader.globalMaximumLOD = (int)lod;
                            }
                        }
                    }
                    GUILayout.EndVertical();
                }
            }
            GUILayout.EndHorizontal();

            GUI.DragWindow(new Rect(0, 0, Screen.width, Screen.height));

        }

        public bool CallFunction(ButtonData item)
        {
            var method = GetType().GetMethod(item.action);
            if (method != null)
            {
                object[] paramList = new object[item.paramList.Count];
                // 判断paramList每个元素的类型, 并转换为对应类型的参数
                for (var i = 0; i < item.paramList.Count; i++)
                {
                    var param = item.paramList[i];
                    switch (param.type)
                    {
                        case ParamType.Int:
                            paramList[i] = param.intValue;
                            break;
                        case ParamType.Float:
                            paramList[i] = param.floatValue;
                            break;
                        case ParamType.String:
                            paramList[i] = param.stringValue;
                            break;
                    }
                }

                object res;
                res = method.Invoke(this, paramList);

                if (res != null)
                    return (bool)res;
            }

            return false;
        }


        public bool SupportsComputeShaders()
        {
            return SystemInfo.supportsComputeShaders;
        }
        public bool ReversedZ()
        {
            return SystemInfo.usesReversedZBuffer;
        }
        public bool DepthTexture()
        {
#if UNITY_PIPELINE_URP

            var cameraData = Camera.main?.GetComponent<UniversalAdditionalCameraData>();
            return cameraData.requiresDepthTexture;
#else
            return false;
#endif
        }
        public bool SupportsTextureFormatASTC()
        {
            return SystemInfo.SupportsTextureFormat(TextureFormat.ASTC_4x4);
        }
        // ================================ Button Function ================================

        public bool FunctionTest()
        {
            Debug.Log("Function Test");
            return false;
        }
    }
}
