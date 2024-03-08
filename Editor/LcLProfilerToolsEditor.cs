using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;

namespace LcLTools
{
    [CustomEditor(typeof(LcLProfiler))]
    public class LcLProfilerToolsEditor : Editor
    {
        // create SerializedProperty
        private SerializedProperty targetFrameRateProp;
        private SerializedProperty boxWidthProp;
        private SerializedProperty fpsBoxHeightProp;
        private SerializedProperty fpsFontSizeProp;
        private SerializedProperty infoBoxHeightProp;
        private SerializedProperty infoFontSizeProp;
        private SerializedProperty powerBoxHeightProp;
        private SerializedProperty powerSizeProp;
        private SerializedProperty cpuGpuInfoActiveProp;
        private SerializedProperty powerActiveProp;
        private SerializedProperty GCMemoryActiveProp;
        private SerializedProperty systemMemoryActiveProp;
        private SerializedProperty setPassCallsActiveProp;
        private SerializedProperty drawCallsActiveProp;
        private SerializedProperty trianglesActiveProp;
        private SerializedProperty verticesActiveProp;
        private SerializedProperty enableSRPBatcherProfilerProp;
        private SerializedProperty srpBoxSizeProp;
        private SerializedProperty srpBoxHeightProp;
        private SerializedProperty srpFontSizeProp;


        private void OnEnable()
        {
            // FindProperty
            targetFrameRateProp = serializedObject.FindProperty("targetFrameRate");
            boxWidthProp = serializedObject.FindProperty("boxWidth");
            fpsBoxHeightProp = serializedObject.FindProperty("fpsBoxHeight");
            fpsFontSizeProp = serializedObject.FindProperty("fpsFontSize");
            infoBoxHeightProp = serializedObject.FindProperty("infoBoxHeight");
            infoFontSizeProp = serializedObject.FindProperty("infoFontSize");
            powerBoxHeightProp = serializedObject.FindProperty("powerBoxHeight");
            powerSizeProp = serializedObject.FindProperty("powerSize");
            cpuGpuInfoActiveProp = serializedObject.FindProperty("cpuGpuInfoActive");
            powerActiveProp = serializedObject.FindProperty("powerActive");
            GCMemoryActiveProp = serializedObject.FindProperty("GCMemoryActive");
            systemMemoryActiveProp = serializedObject.FindProperty("systemMemoryActive");
            setPassCallsActiveProp = serializedObject.FindProperty("setPassCallsActive");
            drawCallsActiveProp = serializedObject.FindProperty("drawCallsActive");
            trianglesActiveProp = serializedObject.FindProperty("trianglesActive");
            verticesActiveProp = serializedObject.FindProperty("verticesActive");
            enableSRPBatcherProfilerProp = serializedObject.FindProperty("enableSRPBatcherProfiler");
            srpBoxSizeProp = serializedObject.FindProperty("srpBoxSize");
            srpBoxHeightProp = serializedObject.FindProperty("srpBoxHeight");
            srpFontSizeProp = serializedObject.FindProperty("srpFontSize");

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(targetFrameRateProp);
            EditorGUILayout.PropertyField(boxWidthProp);
            EditorGUILayout.BeginVertical("U2D.createRect");
            {
                EditorGUILayout.PropertyField(fpsBoxHeightProp);
                EditorGUILayout.PropertyField(fpsFontSizeProp);

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.PropertyField(GCMemoryActiveProp);
                    EditorGUILayout.PropertyField(systemMemoryActiveProp);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.PropertyField(setPassCallsActiveProp);
                    EditorGUILayout.PropertyField(drawCallsActiveProp);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.PropertyField(trianglesActiveProp);
                    EditorGUILayout.PropertyField(verticesActiveProp);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            cpuGpuInfoActiveProp.boolValue = EditorGUILayout.BeginFoldoutHeaderGroup(cpuGpuInfoActiveProp.boolValue, "CPU/GPU：");
            if (cpuGpuInfoActiveProp.boolValue)
            {
                EditorGUILayout.PropertyField(infoBoxHeightProp);
                EditorGUILayout.PropertyField(infoFontSizeProp);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            powerActiveProp.boolValue = EditorGUILayout.BeginFoldoutHeaderGroup(powerActiveProp.boolValue, "电量：");
            if (powerActiveProp.boolValue)
            {
                EditorGUILayout.PropertyField(powerBoxHeightProp);
                EditorGUILayout.PropertyField(powerSizeProp);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();


            enableSRPBatcherProfilerProp.boolValue = EditorGUILayout.BeginFoldoutHeaderGroup(enableSRPBatcherProfilerProp.boolValue, "SRPBatcherProfiler：");
            if (enableSRPBatcherProfilerProp.boolValue)
            {
                EditorGUILayout.PropertyField(srpBoxSizeProp);
                EditorGUILayout.PropertyField(srpBoxHeightProp);
                EditorGUILayout.PropertyField(srpFontSizeProp);
            }




            serializedObject.ApplyModifiedProperties();
        }

        [MenuItem("GameObject/LcLTools/Add LcLDebugTools", false, 0)]
        static void AddLcLDebugTools()
        {
            var go = new GameObject("LcLDebugTools", typeof(LcLDebugTools), typeof(LcLProfiler));
            Selection.activeObject = go;
            Undo.RegisterCreatedObjectUndo(go, "AddLcLDebugTools");
        }
    }
}
