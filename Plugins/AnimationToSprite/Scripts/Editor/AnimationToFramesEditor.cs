using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(AnimationToFrames))]
public class AnimationToFramesEditor : Editor
{
    string[] displayTexts;
    int selectDisplayIndex = 0;

    private void OnEnable()
    {
        GameViewUtils.UpdateDisplaySizes();
        selectDisplayIndex = GameViewUtils.FindSize(Screen.width, Screen.height);
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        AnimationToFrames tools = (AnimationToFrames)target;

        EditorGUI.BeginChangeCheck();
        {
            selectDisplayIndex = EditorGUILayout.Popup("Resolution", selectDisplayIndex, GameViewUtils.DisplayTexts);
        }
        if (EditorGUI.EndChangeCheck())
        {
            GameViewUtils.OpenWindow();
            GameViewUtils.SetSize(selectDisplayIndex);
            GameViewUtils.SetMinScale();
        }

        var list = tools.GetAnimatorClip();
        tools.selectIndex = EditorGUILayout.Popup("Select Animation", tools.selectIndex, list.ToArray());

        var clip = tools.GetClipByIndex(tools.selectIndex);
        // 绘制一个box框，显示一些动画信息

        EditorGUILayout.BeginVertical("box");
        {
            var centeredStyle = GUI.skin.GetStyle("Label");
            centeredStyle.alignment = TextAnchor.MiddleCenter;
            if (clip != null)
            {
                GUILayout.Label($"Animation length: {clip.length} seconds", centeredStyle, GUILayout.ExpandWidth(true));
                GUILayout.Label($"Total frames: {Mathf.Ceil(tools.frameRate * clip.length)} frames", centeredStyle,
                    GUILayout.ExpandWidth(true));
            }
            else
            {
                GUILayout.Label("No animation clip found", centeredStyle, GUILayout.ExpandWidth(true));
            }
        }
        EditorGUILayout.EndVertical();

        if (GUILayout.Button("Preview Animation"))
        {
            tools.PreviewAnimation();
            {
            }
        }

        if (GUILayout.Button("Render Frame"))
        {
            GameViewUtils.OpenWindow();
            tools.Capture();
        }
    }


    [MenuItem("Window/LcL/AnimationToFrames")]
    public static void AddTools()
    {
        var select = Selection.activeGameObject;
        if (select == null || select.GetComponent<Animator>() == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a GameObject with an Animator component", "OK");
            return;
        }

        var tools = select.AddComponent<AnimationToFrames>();
    }
}
