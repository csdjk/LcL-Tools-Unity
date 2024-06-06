using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using LcLTools;

public class EditorTest : Editor
{

    [MenuItem("LcLTools/EditorTest")]
    public static void Test()
    {
        LcLEditorUtilities.GetAssetByName<ComputeShader>("TransformPosition");
    }
}
