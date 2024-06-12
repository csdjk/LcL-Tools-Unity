using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

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
    string AnimationName => m_Animations[selectIndex];
    List<string> m_Empty = new List<string>();
    Animator m_Animator;
    RenderTexture m_RenderTexture;
    bool m_IsCapturing = false;
    int m_FrameCount = 0;
    List<string> m_Paths = new List<string>();

    void OnEnable()
    {
        renderCamera = Camera.main ?? FindObjectOfType<Camera>();
        m_Animator = GetComponent<Animator>();
    }

    void OnDisable()
    {
        StopAnimation();
    }

    public List<string> GetAnimatorClip()
    {
        if (m_Animator == null) return m_Empty;

        var controller = m_Animator.runtimeAnimatorController;
        var clips = controller.animationClips;
        m_Animations = clips.Select(clip => clip.name).ToList();
        return m_Animations;
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
        PlayAnimation();
    }

    public void PlayAnimation(int layer = 0, float normalizedTime = 0f)
    {
        Time.captureFramerate = frameRate;
        m_Animator.speed = 1f;
        m_Animator.Play(AnimationName, layer, normalizedTime);
        EditorApplication.update -= UpdateAnimation;
        EditorApplication.update += UpdateAnimation;
    }

    public void StopAnimation()
    {
        EditorApplication.update -= UpdateAnimation;
    }

    private void UpdateAnimation()
    {
        m_Animator.Update(Time.deltaTime);
        if (m_IsCapturing)
        {
            StartCoroutine(CaptureFrame());
        }
    }

    IEnumerator CaptureFrame()
    {
        yield return new WaitForEndOfFrame();

        var path = Path.Combine(outputFolder, $"{AnimationName}_{m_FrameCount}.png");
        m_Paths.Add(path);
        // 捕获屏幕截图，并将其保存为PNG文件
        // ScreenCapture.CaptureScreenshot(path);
        renderCamera.targetTexture = m_RenderTexture;
        renderCamera.Render();
        RenderTexture.active = m_RenderTexture;

        // 创建一个新的 Texture2D，大小和屏幕一样大
        var tex = new Texture2D(Width, Height, TextureFormat.ARGB32, false);
        // 读取屏幕像素
        tex.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
        tex.Apply();

        // 将 Texture2D 编码为 PNG
        byte[] bytes = tex.EncodeToPNG();

        // 保存 PNG 文件
        File.WriteAllBytes(path, bytes);

        // 销毁 Texture2D
        DestroyImmediate(tex);

        m_FrameCount++;
        // 获取 Animator 的当前状态
        AnimatorStateInfo stateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);
        // 检查动画是否播放完成
        if (stateInfo.normalizedTime >= 1)
        {
            // 动画已经播放完成
            CaptureComplete();
        }
    }


    void CaptureComplete()
    {
        m_IsCapturing = false;
        StopAnimation();
        renderCamera.targetTexture = null;
        m_RenderTexture.Release();
        m_RenderTexture = null;
        RenderTexture.active = null;

        //  set import
        AssetDatabase.Refresh();


        foreach (var path in m_Paths)
        {
            SetAlphaIsTransparency(path);
        }
    }

    void SetAlphaIsTransparency(string path)
    {
        // 获取 TextureImporter
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer == null)
        {
            Debug.LogError("importer is null");
            return;
        }

        // 设置 alphaIsTransparency 为 true
        importer.alphaIsTransparency = true;

        // 应用修改
        importer.SaveAndReimport();
    }
}
