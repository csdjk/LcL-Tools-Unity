using UnityEngine;
using UnityEditor;

namespace LcLTools
{
    public class ExtractAnimation : Editor
    {
        [MenuItem("Assets/LcL Extract Animation Clip", false, 1)]
        private static void Extract()
        {
            var selections = Selection.objects;
            foreach (var item in selections)
            {
                var path = AssetDatabase.GetAssetPath(item);
                AnimationClip orgClip = (AnimationClip)AssetDatabase.LoadAssetAtPath(path, typeof(AnimationClip));

                //Save the clip
                AnimationClip placeClip = new AnimationClip();
                EditorUtility.CopySerialized(orgClip, placeClip);
                var parentpath = path.Substring(0, path.LastIndexOf('/'));
                AssetDatabase.CreateAsset(placeClip, parentpath + "/" + item.name + ".anim");
                AssetDatabase.Refresh();
            }
        }
    }
}
