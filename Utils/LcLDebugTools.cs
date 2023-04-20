using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Experimental.Rendering;
using System.Reflection;


namespace LcLTools
{
    public enum LodLevel
    {
        LOD100 = 100,
        LOD200 = 200,
        LOD300 = 300,
    }

    [Serializable]
    public class ButtonData
    {
        public bool active;
        public string name;
        public string action;
        public bool buttonState;
        public string data;
    }
    [Serializable]
    public struct SceneData
    {
        public bool active;
        public string name;
        public int index;
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
        //---------------------------GUI-------------------------------------
        public LodLevel lodLevel = LodLevel.LOD300;

        public PostProcess postProcess;

        public List<SceneData> sceneList;
        public GameObject[] singleList;
        public GameObject[] toggleList;

        [Header("按钮列表")]
        [SerializeField, HideInInspector]
        private List<ButtonData> buttonDataList;

        private GUIStyle enableStyle;
        private GUIStyle disableStyle;

        private void Awake()
        {
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }
        }


        void OnDisable()
        {

        }

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

        private bool featureActive;
        public void PostProcessSwitch()
        {
            featureActive = !featureActive;
            postProcess.postAsset.GetEffect<BloomEffect>().SetActive(featureActive);
        }

        private bool postActive;
        public void PostSwitch()
        {
            if (gameObject.TryGetComponent(out UniversalAdditionalCameraData camData))
            {
                camData.renderPostProcessing = !camData.renderPostProcessing;
                postActive = camData.renderPostProcessing;
            }

            postProcess.FinalFeature.SetActive(!postActive);
        }

        private bool bloomFeatureActive;
        public string GetBloomFeatureState()
        {
            return bloomFeatureActive ? "BloomF(ing...)" : "BloomF";
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

        void OnValidate()
        {
            Shader.globalMaximumLOD = (int)lodLevel;
        }


        bool isInit = true;
        void OnGUI()
        {
            if (uiBoxRect == null)
            {
                return;
            }
            if (isInit)
            {
                isInit = false;
                uiBoxRect.x = Screen.width - uiBoxSize.x;
                uiBoxRect.y = Screen.height - uiBoxSize.y;
                enableStyle = new GUIStyle(GUI.skin.button);
                disableStyle = new GUIStyle(GUI.skin.button);
                enableStyle.normal.textColor = Color.green;
                enableStyle.hover.textColor = Color.green;
                enableStyle.fontSize = fontSize;
                disableStyle.normal.textColor = Color.white;
                disableStyle.fontSize = fontSize;
            }

            uiBoxRect = GUI.Window(windowID, uiBoxRect, WindowCallBack, "");
            uiBoxRect.width = uiBoxSize.x;
            uiBoxRect.height = uiBoxSize.y;
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


                GUILayout.BeginVertical();
                {
                    GUILayout.Label($"ReversedZ: {SystemInfo.usesReversedZBuffer}", GetStyle(SystemInfo.usesReversedZBuffer));
                    GUILayout.Space(10);
                    foreach (LodLevel lod in Enum.GetValues(typeof(LodLevel)))
                    {
                        if (Button(lod.ToString(), Shader.globalMaximumLOD == (int)lod))
                        {
                            Shader.globalMaximumLOD = (int)lod;
                        }
                    }
                    if (Button(GetSRPState()))
                    {
                        SRPSwitch();
                    }

                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();

            GUI.DragWindow(new Rect(0, 0, Screen.width, Screen.height));

        }

        public bool CallFunction(ButtonData item)
        {
            var method = GetType().GetMethod(item.action);
            if (method != null)
            {
                object res;
                // 判断方法名是否等于 SwitchKeyword ,如果是就传递参数调用
                if (item.action == "SwitchKeyword")
                {
                    res = method.Invoke(this, new object[] { item.data });
                    return (bool)res;
                }
                res = method.Invoke(this, null);
                if (res != null)
                    return (bool)res;
            }

            return false;
        }

        // ================================ Button Function ================================
        public bool EnableBlur()
        {
            var active = postProcess.GetEffectActive<BlurEffect>();
            postProcess.SetEffectActive<BlurEffect>(!active);
            return !active;
        }
        public bool EnableBloom()
        {
            var active = postProcess.GetEffectActive<BloomEffect>();
            postProcess.SetEffectActive<BloomEffect>(!active);
            return !active;
        }
        public bool EnableDof()
        {
            var active = postProcess.GetEffectActive<DepthOfFieldEffect>();
            postProcess.SetEffectActive<DepthOfFieldEffect>(!active);
            return !active;
        }
        public bool EnableLensFlare()
        {
            var active = postProcess.GetEffectActive<LensFlareEffect>();
            postProcess.SetEffectActive<LensFlareEffect>(!active);
            return !active;
        }

        public bool EnableVolumetricLight()
        {
            var active = postProcess.GetEffectActive<VolumetricLightEffect>();
            postProcess.SetEffectActive<VolumetricLightEffect>(!active);
            return !active;
        }


        // public bool EnableLoop()
        // {
        //     var active = postProcess.GetEffectSettings<VolumetricLightEffect>() as VolumetricLightEffect.Setting;
        //     active.loop = !active.loop;
        //     return active.loop;
        // }
    }
}
