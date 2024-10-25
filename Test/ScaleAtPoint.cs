using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class ScaleAtPoint : MonoBehaviour
{
    public Vector3 scale = Vector3.one;
    public void Update()
    {
        transform.localScale = scale;
    }
}


[CustomEditor(typeof(ScaleAtPoint))]
[CanEditMultipleObjects]
public class ScaleAtPointEditor : Editor
{
    public void OnSceneGUI()
    {
        ScaleAtPoint t = (target as ScaleAtPoint);

        EditorGUI.BeginChangeCheck();
        Vector3 scale = Handles.ScaleHandle(t.scale, t.transform.position, Quaternion.identity, 1);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Scaled ScaleAt Point");
            t.scale = scale;
            t.Update();
        }
    }
}
