using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection.Emit;
using System;
using System.Reflection;
using UnityEditor;

// https://forum.unity.com/threads/access-a-materials-saved-properties-via-script.475227/
/// <summary>
/// 修复材质球属性，将旧版材质球属性转换为新版材质球属性（修改Shader变量名导致的） 
/// </summary>
public class FixMaterials
{
    [MenuItem("LcLTools/FixMaterials")]
    public static void UpdateOldProperties()
    {
        foreach (var mat in Selection.GetFiltered<Material>(SelectionMode.Assets))
        {
            Debug.Log(mat.name);
            FixColor(mat, "_Tint", "_Color");
        }
    }

    private static void FixFloat(Material mat, string oldName, string newName)
    {
        if (mat.HasProperty(newName))
        {
            var so = new SerializedObject(mat);
            var itr = so.GetIterator();
            while (itr.Next(true))
            {
                if (itr.displayName == oldName)
                {
                    if (itr.hasChildren)
                    {
                        var itrC = itr.Copy();
                        itrC.Next(true); //Walk into child ("First")
                        itrC.Next(false); //Walk into sibling ("Second")
                        mat.SetFloat(newName, itrC.floatValue);
                    }
                }
            }
        }
    }

    private static void FixColor(Material mat, string oldName, string newName)
    {
        if (mat.HasProperty(newName))
        {
            var so = new SerializedObject(mat);
            var itr = so.GetIterator();
            while (itr.Next(true))
            {
                if (itr.displayName == oldName)
                {
                    if (itr.hasChildren)
                    {
                        var itrC = itr.Copy();
                        itrC.Next(true); //Walk into child ("First")
                        itrC.Next(false); //Walk into sibling ("Second")
                        mat.SetColor(newName, itrC.colorValue);
                    }
                }
            }
        }
    }
}