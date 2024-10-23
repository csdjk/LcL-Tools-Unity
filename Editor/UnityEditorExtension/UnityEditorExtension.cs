using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LcLTools.UnityEditorExtension
{
    public static class UnityEditorExtension
    {

        public static void DefaultShaderPropertyInternal(this MaterialEditor editor, Rect position, MaterialProperty prop, GUIContent label)
        {
            editor.DefaultShaderPropertyInternal(position, prop, label);
        }
    }
}
