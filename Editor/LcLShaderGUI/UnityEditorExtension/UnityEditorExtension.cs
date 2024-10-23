using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LcLShaderEditor
{
    public static class UnityEditorExtension
    {
        public static void DefaultShaderPropertyInternal(this MaterialEditor editor, Rect position, MaterialProperty prop, GUIContent label)
        {
            editor.DefaultShaderPropertyInternal(position, prop, label);
        }

        public static void DefaultShaderPropertyInternal(this MaterialEditor editor, MaterialProperty prop, GUIContent label)
        {
            editor.DefaultShaderPropertyInternal(prop, label);
        }
    }
}