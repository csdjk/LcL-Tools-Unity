using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


public static class GameViewUtils
{
    public enum GameViewSizeType
    {
        AspectRatio,
        FixedResolution
    }

    static object m_GameViewSizesInstance;
    static Type m_GameViewType;
    static Type m_GameViewSizesType;
    static Type m_GameViewSizeType;
    static Type m_GameViewSizeTypeType;

    static GameViewSizeGroupType m_CurrentGroupType;

    public static EditorWindow GameViewWindow => EditorWindow.GetWindow(m_GameViewType);
    public static GameViewSizeGroupType CurrentGroupType => m_CurrentGroupType;
    public static object CurrentGameViewSize => ReflectionUtils.GetProperty(GameViewWindow, "currentGameViewSize");
    public static object CurrentGroup => ReflectionUtils.GetProperty(m_GameViewSizesInstance, "currentGroup");
    public static float minScale => (float)ReflectionUtils.GetProperty(GameViewWindow, "minScale");
    static string[] m_DisplayTexts;
    public static string[] DisplayTexts => m_DisplayTexts;
    static List<Vector2Int> m_DisplaySizes = new List<Vector2Int>();
    public static List<Vector2Int> DisplaySizes => m_DisplaySizes;

    static GameViewUtils()
    {
        // m_GameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");
        m_GameViewType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
        m_GameViewSizesType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");
        m_GameViewSizeType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");
        m_GameViewSizeTypeType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizeType");

        var singleType = typeof(ScriptableSingleton<>).MakeGenericType(m_GameViewSizesType);
        var instanceProp = singleType.GetProperty("instance");
        m_GameViewSizesInstance = instanceProp.GetValue(null, null);
        m_CurrentGroupType = GetCurrentGroupType();

        UpdateDisplaySizes();
    }

    public static EditorWindow OpenWindow()
    {
        return EditorWindow.GetWindow(m_GameViewType);
    }

    [MenuItem("Test/SnapZoom")]
    public static void SnapZoomMin()
    {
        SetSize(256, 256);
        SnapZoom(0.1f);
    }

    public static void SetMinScale()
    {
        SnapZoom((minScale));
    }

    public static void SnapZoom(float zoom = 1f)
    {
        //延迟调用
        EditorApplication.delayCall += () => { ReflectionUtils.Invoke(GameViewWindow, "SnapZoom", zoom); };
    }

    public static void AddVisualElementToGameView(VisualElement box)
    {
        var rootVisualElement = GameViewWindow.rootVisualElement;
        rootVisualElement.Add(box);
    }

    public static void RemoveVisualElementFromGameView(VisualElement box)
    {
        var rootVisualElement = GameViewWindow.rootVisualElement;
        rootVisualElement.Remove(box);
    }

    public static void SetSize(int width, int height, GameViewSizeType viewSizeType = GameViewSizeType.FixedResolution,
        string text = "")
    {
        UpdateDisplaySizes();
        int idx = FindSize(width, height);
        if (idx != -1)
            SetSize(idx);
        else
            AddAndSelectCustomSize(width, height, text, viewSizeType);
    }

    public static void SetSize(int index, GameViewSizeType viewSizeType = GameViewSizeType.FixedResolution)
    {
        ReflectionUtils.SetProperty(GameViewWindow, "selectedSizeIndex", index);
    }

    public static void AddAndSelectCustomSize(int width, int height, string text, GameViewSizeType viewSizeType)
    {
        AddCustomSize(width, height, text, viewSizeType);
        int idx = FindSize(width, height);
        SetSize(idx);
    }

    public static void AddCustomSize(int width, int height, string text, GameViewSizeType viewSizeType)
    {
        ConstructorInfo ctor = m_GameViewSizeType.GetConstructor(new Type[]
        {
            m_GameViewSizeTypeType,
            typeof(int),
            typeof(int),
            typeof(string)
        });
        var newSize = ctor.Invoke(new object[] { (int)viewSizeType, width, height, text });

        ReflectionUtils.Invoke(CurrentGroup, "AddCustomSize", newSize);

        UpdateDisplaySizes();
    }

    public static bool SizeExists(string text)
    {
        return FindSize(text) != -1;
    }

    public static int FindSize(string text)
    {
        for (int i = 0; i < DisplayTexts.Length; i++)
        {
            string display = DisplayTexts[i];
            int pren = display.IndexOf('(');
            if (pren != -1)
                display = display.Substring(0, pren - 1);
            if (display == text)
                return i;
        }

        return -1;
    }

    public static bool SizeExists(int width, int height)
    {
        return FindSize(width, height) != -1;
    }

    public static int FindSize(int width, int height)
    {
        for (int i = 0; i < DisplaySizes.Count; i++)
        {
            if (DisplaySizes[i].x == width && DisplaySizes[i].y == height)
                return i;
        }

        return -1;
    }

    public static void UpdateDisplaySizes()
    {
        var builtInCount = (int)ReflectionUtils.Invoke(CurrentGroup, "GetBuiltinCount");
        var customCount = (int)ReflectionUtils.Invoke(CurrentGroup, "GetCustomCount");
        int sizesCount = builtInCount + customCount;

        m_DisplaySizes.Clear();
        for (int i = 0; i < sizesCount; i++)
        {
            var size = ReflectionUtils.Invoke(CurrentGroup, "GetGameViewSize", i);
            int sizeWidth = (int)ReflectionUtils.GetProperty(size, "width");
            int sizeHeight = (int)ReflectionUtils.GetProperty(size, "height");
            m_DisplaySizes.Add(new Vector2Int(sizeWidth, sizeHeight));
        }

        m_DisplayTexts = ReflectionUtils.Invoke(CurrentGroup, "GetDisplayTexts") as string[];
    }


    static object GetGroup(GameViewSizeGroupType type)
    {
        return ReflectionUtils.Invoke(m_GameViewSizesInstance, "GetGroup", type);
    }

    static GameViewSizeGroupType GetCurrentGroupType()
    {
        var getCurrentGroupTypeProp = m_GameViewSizesInstance.GetType().GetProperty("currentGroupType");
        return (GameViewSizeGroupType)(int)getCurrentGroupTypeProp.GetValue(m_GameViewSizesInstance, null);
    }
}
