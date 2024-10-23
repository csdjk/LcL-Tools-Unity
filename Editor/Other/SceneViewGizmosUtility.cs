using UnityEditor;

public static class SceneViewGizmosUtility
{
    [MenuItem("LcLTools/HotKeys/Scene View Gizmos &g")]
    public static void ToggleSceneViewGizmos()
    {
        var currentValue = GetSceneViewGizmosEnabled();
        SetSceneViewGizmos(!currentValue);
    }

    public static void SetSceneViewGizmos(bool gizmosOn)
    {
#if UNITY_EDITOR
        UnityEditor.SceneView sv =
            UnityEditor.EditorWindow.GetWindow<UnityEditor.SceneView>(null, false);
        sv.drawGizmos = gizmosOn;
#endif
    }

    public static bool GetSceneViewGizmosEnabled()
    {
#if UNITY_EDITOR
        UnityEditor.SceneView sv =
            UnityEditor.EditorWindow.GetWindow<UnityEditor.SceneView>(null, false);
        return sv.drawGizmos;
#else
        return false;
#endif
    }
}
