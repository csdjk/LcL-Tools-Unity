using System;
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
    static MethodInfo m_GetGroup;
    static Type m_GameViewType;
    static GameViewSizeGroupType m_CurrentGroupType;
    public static GameViewSizeGroupType CurrentGroupType => m_CurrentGroupType;

    static string[] m_DisplayTexts;
    public static string[] DisplayTexts => m_DisplayTexts;
    static Vector2Int[] m_DisplaySizes = new Vector2Int[0];
    public static Vector2Int[] DisplaySizes => m_DisplaySizes;

    static GameViewUtils()
    {
        // gameViewSizesInstance  = ScriptableSingleton<GameViewSizes>.instance;
        m_GameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");
        // m_GameViewType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");

        var sizesType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");
        var singleType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
        var instanceProp = singleType.GetProperty("instance");
        m_GetGroup = sizesType.GetMethod("GetGroup");
        m_GameViewSizesInstance = instanceProp.GetValue(null, null);
        m_CurrentGroupType = GetCurrentGroupType();

        UpdateDisplaySizes();
    }


    public static EditorWindow GetWindow()
    {
        return EditorWindow.GetWindow(m_GameViewType);
    }

    public static void AddVisualElementToGameView(VisualElement box)
    {
        var gameViewWindow = GetWindow();
        var rootVisualElement = gameViewWindow.rootVisualElement;
        rootVisualElement.Add(box);
    }

    public static void RemoveVisualElementFromGameView(VisualElement box)
    {
        var gameViewWindow = GetWindow();
        var rootVisualElement = gameViewWindow.rootVisualElement;
        rootVisualElement.Remove(box);
    }

    public static void SetSize(int width, int height, GameViewSizeType viewSizeType = GameViewSizeType.FixedResolution,
        string text = "")
    {
        int idx = FindSize(width, height);
        if (idx != -1)
            SetSize(idx);
        else
            AddAndSelectCustomSize(width, height, text, viewSizeType);
    }

    public static void SetSize(int index, GameViewSizeType viewSizeType = GameViewSizeType.FixedResolution)
    {
        var gvWndType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
        var selectedSizeIndexProp = gvWndType.GetProperty("selectedSizeIndex",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var gvWnd = EditorWindow.GetWindow(gvWndType);
        if (selectedSizeIndexProp != null) selectedSizeIndexProp.SetValue(gvWnd, index);
    }

    // [MenuItem("Test/SizeDimensionsQuery")]
    public static void SizeDimensionsQueryTest()
    {
        Debug.Log(SizeExists(123, 456));
    }

    public static void AddAndSelectCustomSize(int width, int height, string text, GameViewSizeType viewSizeType)
    {
        AddCustomSize(width, height, text, viewSizeType);
        int idx = GameViewUtils.FindSize(width, height);
        GameViewUtils.SetSize(idx);
    }

    public static void AddCustomSize(int width, int height, string text, GameViewSizeType viewSizeType)
    {
        var group = GetGroup();
        var addCustomSize = m_GetGroup.ReturnType.GetMethod("AddCustomSize"); // or group.GetType().
        var gvsType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");
        string assemblyName = "UnityEditor.dll";
        Assembly assembly = Assembly.Load(assemblyName);
        Type gameViewSize = assembly.GetType("UnityEditor.GameViewSize");
        Type gameViewSizeType = assembly.GetType("UnityEditor.GameViewSizeType");
        ConstructorInfo ctor = gameViewSize.GetConstructor(new Type[]
        {
            gameViewSizeType,
            typeof(int),
            typeof(int),
            typeof(string)
        });
        var newSize = ctor.Invoke(new object[] { (int)viewSizeType, width, height, text });
        addCustomSize.Invoke(group, new object[] { newSize });
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
            // the text we get is "Name (W:H)" if the size has a name, or just "W:H" e.g. 16:9
            // so if we're querying a custom size text we substring to only get the name
            // You could see the outputs by just logging
            // Debug.Log(display);
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
        for (int i = 0; i < DisplaySizes.Length; i++)
        {
            if (DisplaySizes[i].x == width && DisplaySizes[i].y == height)
                return i;
        }

        return -1;
    }

    public static void UpdateDisplaySizes()
    {
        var group = GetGroup(CurrentGroupType);
        var groupType = group.GetType();
        var getBuiltinCount = groupType.GetMethod("GetBuiltinCount");
        var getCustomCount = groupType.GetMethod("GetCustomCount");
        int sizesCount = (int)getBuiltinCount.Invoke(group, null) + (int)getCustomCount.Invoke(group, null);
        var getGameViewSize = groupType.GetMethod("GetGameViewSize");
        var gvsType = getGameViewSize.ReturnType;
        var widthProp = gvsType.GetProperty("width");
        var heightProp = gvsType.GetProperty("height");
        var indexValue = new object[1];
        m_DisplaySizes = new Vector2Int[sizesCount];
        for (int i = 0; i < sizesCount; i++)
        {
            indexValue[0] = i;
            var size = getGameViewSize.Invoke(group, indexValue);
            int sizeWidth = (int)widthProp.GetValue(size, null);
            int sizeHeight = (int)heightProp.GetValue(size, null);
            m_DisplaySizes[i] = new Vector2Int(sizeWidth, sizeHeight);
        }


        var getDisplayTexts = group.GetType().GetMethod("GetDisplayTexts");
        m_DisplayTexts = getDisplayTexts.Invoke(group, null) as string[];
    }

    static object GetGroup(GameViewSizeGroupType? type = null)
    {
        type = type ?? m_CurrentGroupType;
        return m_GetGroup.Invoke(m_GameViewSizesInstance, new object[] { (int)type });
    }

    [MenuItem("Test/LogCurrentGroupType")]
    public static void LogCurrentGroupType()
    {
        Debug.Log(GetCurrentGroupType());
    }

    static GameViewSizeGroupType GetCurrentGroupType()
    {
        var getCurrentGroupTypeProp = m_GameViewSizesInstance.GetType().GetProperty("currentGroupType");
        return (GameViewSizeGroupType)(int)getCurrentGroupTypeProp.GetValue(m_GameViewSizesInstance, null);
    }
}
