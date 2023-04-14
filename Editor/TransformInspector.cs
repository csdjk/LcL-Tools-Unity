// Alternative version, with redundant code removed
using UnityEngine;
using UnityEditor;
using System.Collections;
// [CustomEditor(typeof(Transform))]
public class TransformInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Transform t = (Transform)target;
     
        if (GUILayout.Button("Reset Transforms"))
        {
            Undo.RegisterCompleteObjectUndo(t, "Reset Transforms " + t.name);
            t.position = Vector3.zero;
            t.rotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }
    }
    private Vector3 FixIfNaN(Vector3 v)
    {
        if (float.IsNaN(v.x))
        {
            v.x = 0;
        }
        if (float.IsNaN(v.y))
        {
            v.y = 0;
        }
        if (float.IsNaN(v.z))
        {
            v.z = 0;
        }
        return v;
    }
}