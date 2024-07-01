using System;
using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;

[ExecuteInEditMode]
public class AnimationToFrames : MonoBehaviour
{
    public Camera renderCamera;
    public string outputFolder = "Assets/3Dto2D/Animation";
    public int Width => renderCamera.pixelWidth;
    public int Height => renderCamera.pixelHeight;

    public int frameRate = 16;
    List<string> m_Animations;
    [HideInInspector] public int selectIndex;
    public string AnimationName => m_Animations[selectIndex];
    public AnimationClip[] Clips => m_Controller?.animationClips;
    List<string> m_Empty = new List<string>();
    Animator m_Animator;
    RuntimeAnimatorController m_Controller;
    RenderTexture m_RenderTexture;
    bool m_IsCapturing = false;
    int m_FrameCount = 0;
    int m_CurrentLayler = 0;
    string m_CurrentStateName;
    List<string> m_Paths = new List<string>();

    void OnEnable()
    {
        renderCamera = Camera.main ? Camera.main : FindObjectOfType<Camera>();
        m_Animator = GetComponent<Animator>();
        m_Controller = m_Animator.runtimeAnimatorController;
    }

    void OnDisable()
    {
        StopAnimation();
    }

    public AnimationClip GetClipByIndex(int index)
    {
        return Clips == null || Clips.Length == 0 ? null : Clips[index];
    }

    public List<string> GetAnimatorClip()
    {
        if (m_Animator == null) return m_Empty;
        m_Controller = m_Animator.runtimeAnimatorController;
        if (m_Controller == null) return m_Empty;

        var clips = m_Controller.animationClips;
        m_Animations = clips.Select(clip => clip.name).ToList();
        return m_Animations;
    }

    public void PreviewAnimation()
    {
        m_IsCapturing = false;
        PlayAnimationByClipName(AnimationName);
    }

    public void Capture()
    {
        if (renderCamera == null || m_Animator == null)
        {
            return;
        }

        // 设置动画的帧率
        Time.captureFramerate = frameRate;
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        if (m_RenderTexture != null)
        {
            m_RenderTexture.Release();
        }

        m_RenderTexture = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);
        // 开始捕获序列帧动画
        m_IsCapturing = true;
        m_FrameCount = 0;
        m_Paths.Clear();
        PlayAnimationByClipName(AnimationName);
    }

    public void PlayAnimation(string stateName, int layer = 0, float normalizedTime = 0f)
    {
        Time.captureFramerate = frameRate;
        m_Animator.speed = 1f;
        m_Animator.Play(stateName, layer, normalizedTime);
        EditorApplication.update -= UpdateAnimation;
        EditorApplication.update += UpdateAnimation;
        // 记录动画开始播放的时间
        m_AnimationStartTime = Time.time;
        m_AnimationLength = m_Animator.GetCurrentAnimatorStateInfo(m_CurrentLayler).length;
    }

    void InitAnimationInfo(string clipName)
    {
        AnimatorController ac = m_Controller as AnimatorController;
        if(ac == null) return;
        m_CurrentStateName = String.Empty;
        m_CurrentLayler = 0;
        for (var i = 0; i < ac.layers.Length; i++)
        {
            var layer = ac.layers[i];
            var stateMachine = layer.stateMachine;
            foreach (var state in stateMachine.states)
            {
                if (state.state.motion is AnimationClip clip && clip.name == clipName)
                {
                    m_CurrentStateName = state.state.name;
                    m_CurrentLayler = i;
                    break;
                }
            }

            if (m_CurrentStateName.Equals(String.Empty)) break;
        }
    }

    public void PlayAnimationByClipName(string clipName, float normalizedTime = 0f)
    {
        InitAnimationInfo(clipName);
        if (!m_CurrentStateName.Equals(String.Empty))
        {
            Time.captureFramerate = frameRate;
            m_Animator.speed = 1f;
            m_Animator.Play(m_CurrentStateName, m_CurrentLayler, normalizedTime);
            EditorApplication.update -= UpdateAnimation;
            EditorApplication.update += UpdateAnimation;
            // 记录动画开始播放的时间
            m_AnimationStartTime = Time.time;
            m_AnimationLength = m_Animator.GetCurrentAnimatorStateInfo(m_CurrentLayler).length;
            // Debug.Log($"PlayAnimationByClipName: {clipName},length: {animationLength}");
        }
        else
        {
            Debug.LogError("Animation clip not found in any state: " + clipName);
        }
    }

    public void StopAnimation()
    {
        EditorApplication.update -= UpdateAnimation;
    }

    float m_AnimationStartTime;
    float m_AnimationLength;

    private void UpdateAnimation()
    {
        // Debug.Log(Time.deltaTime);
        m_Animator.Update(Time.deltaTime);
        if (m_IsCapturing)
        {
            StartCoroutine(CaptureFrame());
        }


        if (Time.time - m_AnimationStartTime >= m_AnimationLength)
        {
            StopAnimation();
        }
    }

    IEnumerator CaptureFrame()
    {
        yield return new WaitForEndOfFrame();

        var path = Path.Combine(outputFolder, $"{AnimationName}_{m_FrameCount}.png");
        m_Paths.Add(path);
        // 捕获屏幕截图，并将其保存为PNG文件
        renderCamera.targetTexture = m_RenderTexture;
        renderCamera.Render();
        RenderTexture.active = m_RenderTexture;
        // 读取屏幕像素
        var tex = new Texture2D(Width, Height, TextureFormat.ARGB32, false);
        tex.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
        tex.Apply();

        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        DestroyImmediate(tex);

        m_FrameCount++;
        if (Time.time - m_AnimationStartTime >= m_AnimationLength)
        {
            // 动画已经播放完成
            CaptureComplete();
            // StopAnimation();
        }
        // if (stateInfo.normalizedTime >= 1)
        // {
        //     // 动画已经播放完成
        //     CaptureComplete();
        // }
    }


    void CaptureComplete()
    {
        m_IsCapturing = false;
        StopAnimation();
        renderCamera.targetTexture = null;
        m_RenderTexture.Release();
        m_RenderTexture = null;
        RenderTexture.active = null;

        AssetDatabase.Refresh();

        foreach (var path in m_Paths)
        {
            SetTextureImporter(path);
        }
    }

    void SetTextureImporter(string path)
    {
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer == null)
        {
            Debug.LogError("importer is null");
            return;
        }

        importer.alphaIsTransparency = true;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.SaveAndReimport();
    }
}
