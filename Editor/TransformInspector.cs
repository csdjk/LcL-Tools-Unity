using UnityEngine;
using UnityEditor;
using System.Collections;
namespace UnityEditor
{
    [CustomEditor(typeof(Transform))]
    [CanEditMultipleObjects]
    internal class TransformInspector : Editor
    {
        GUIStyle m_IconButtonStyle;
        GUIStyle iconButtonStyle
        {
            get
            {
                if (m_IconButtonStyle == null)
                {
                    m_IconButtonStyle = new GUIStyle(EditorStyles.miniButton);
                    m_IconButtonStyle.margin = new RectOffset(0, 0, 0, 0);
                    m_IconButtonStyle.fixedHeight = 0;
                }
                return m_IconButtonStyle;
            }
        }
        SerializedProperty m_Position;
        SerializedProperty m_Scale;
        SerializedProperty m_Rotation;

        // TransformRotationGUI m_RotationGUI;
        // ConstrainProportionsTransformScale m_ConstrainProportionsScale;
        SerializedProperty m_ConstrainProportionsScaleProperty;
        bool m_IsScaleDirty;

        class Contents
        {
            public GUIContent positionContent = EditorGUIUtility.TrTextContent("Position", "The local position of this GameObject relative to the parent.");
            public GUIContent scaleContent = EditorGUIUtility.TrTextContent("Scale", "The local scaling of this GameObject relative to the parent.");
            public GUIContent rotationContent = EditorGUIUtility.TrTextContent("Rotation", "The local rotation of this GameObject relative to the parent.");
            public string floatingPointWarning = "Due to floating-point precision limitations, it is recommended to bring the world coordinates of the GameObject within a smaller range.";
        }
        static Contents s_Contents;

        public void OnEnable()
        {
            // this.positionProperty = this.serializedObject.FindProperty("m_LocalPosition");
            // this.rotationProperty = this.serializedObject.FindProperty("m_LocalRotation");
            // this.scaleProperty = this.serializedObject.FindProperty("m_LocalScale");

            m_Position = serializedObject.FindProperty("m_LocalPosition");
            m_Scale = serializedObject.FindProperty("m_LocalScale");
            m_Rotation = serializedObject.FindProperty("m_LocalRotation");

            m_ConstrainProportionsScaleProperty = serializedObject.FindProperty("m_ConstrainProportionsScale");

            // if (m_RotationGUI == null)
            // m_RotationGUI = new TransformRotationGUI();
            // m_RotationGUI.OnEnable(serializedObject.FindProperty("m_LocalRotation"), EditorGUIUtility.TrTextContent("Rotation", "The local rotation of this GameObject relative to the parent."));
            // m_ConstrainProportionsScale = new ConstrainProportionsTransformScale(m_Scale.vector3Value);
        }

        public override void OnInspectorGUI()
        {
            if (s_Contents == null)
                s_Contents = new Contents();

            if (!EditorGUIUtility.wideMode)
            {
                EditorGUIUtility.wideMode = true;
                EditorGUIUtility.labelWidth = EditorGUIUtility.currentViewWidth - 212;
            }

            serializedObject.Update();

            Inspector3D();

            // If scale is dirty, update serializedObject to avoid overwriting new value with previous one when applying modifications
            if (m_IsScaleDirty)
                serializedObject.Update();

            Transform t = target as Transform;
            Vector3 pos = t.position;
            if (Mathf.Abs(pos.x) > 100000 || Mathf.Abs(pos.y) > 100000 || Mathf.Abs(pos.z) > 100000)
                EditorGUILayout.HelpBox(s_Contents.floatingPointWarning, MessageType.Warning);

            bool saveUndoName = serializedObject.hasModifiedProperties;
            serializedObject.ApplyModifiedProperties();

            if (saveUndoName)
            {
                int selection = Selection.count;
                if (t.position != pos)
                    Undo.SetCurrentGroupName(string.Format("Set Position to {0} in {1}", t.localPosition, selection == 1 ? target.name : "Selected Objects"));
                else
                    Undo.SetCurrentGroupName(string.Format("Set Scale to {0} in {1}", t.localScale, selection == 1 ? target.name : "Selected Objects"));
            }

            if (m_IsScaleDirty)
            {
                m_IsScaleDirty = false;
                // OnForceReloadInspector();
            }
            //resetting label width as it is carried over to other windows
            EditorGUIUtility.labelWidth = 0;
        }

        void Inspector3D()
        {
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.PropertyField(m_Position, s_Contents.positionContent);

                if (GUILayout.Button(EditorGUIUtility.IconContent("Refresh"), GUILayout.Width(20)))
                {
                    m_Position.vector3Value = Vector3.zero;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.PropertyField(m_Rotation, s_Contents.rotationContent);

                if (GUILayout.Button(EditorGUIUtility.IconContent("Refresh"), GUILayout.Width(20)))
                {
                    m_Rotation.quaternionValue = Quaternion.identity;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.PropertyField(m_Scale, s_Contents.scaleContent);
                if (GUILayout.Button(EditorGUIUtility.IconContent("Refresh"), GUILayout.Width(20)))
                {
                    m_Scale.vector3Value = Vector3.one;
                }
            }
            GUILayout.EndHorizontal();

            Transform t = target as Transform;
            // var mixedFields = ConstrainProportionsTransformScale.GetMixedValueFields(m_Scale);
            // if (t != null && m_ConstrainProportionsScale.Initialize(serializedObject.targetObjects) && m_ConstrainProportionsScaleProperty != null)
            // {
            //     //AxisModified values [-1;2] : [none, x, y, z]
            //     int axisModified = -1;
            //     var mixedFields = ConstrainProportionsTransformScale.GetMixedValueFields(m_Scale);
            //     Vector3 currentScale = m_ConstrainProportionsScale.DoGUI(EditorGUILayout.GetControlRect(true),
            //         s_Contents.scaleContent, m_Scale.vector3Value, serializedObject.targetObjects, ref axisModified, m_Scale, m_ConstrainProportionsScaleProperty);
            //     var mixedFieldsAfterGUI = ConstrainProportionsTransformScale.GetMixedValueFields(m_Scale);

            //     if (currentScale != m_Scale.vector3Value || mixedFields != mixedFieldsAfterGUI)
            //     {
            //         if (serializedObject.targetObjectsCount > 1)
            //         {
            //             if (mixedFields != mixedFieldsAfterGUI)
            //             {
            //                 axisModified = -1;
            //                 for (int i = 0; i < 3; i++)
            //                 {
            //                     if (ConstrainProportionsTransformScale.IsBit(mixedFields, i) && !ConstrainProportionsTransformScale.IsBit(mixedFieldsAfterGUI, i))
            //                     {
            //                         axisModified = i;
            //                         break;
            //                     }
            //                 }
            //             }

            //             if (axisModified != -1)
            //                 m_IsScaleDirty = ConstrainProportionsTransformScale.HandleMultiSelectionScaleChanges(
            //                     m_Scale.vector3Value,
            //                     currentScale, m_ConstrainProportionsScale.constrainProportionsScale,
            //                     serializedObject.targetObjects, ref axisModified);
            //         }

            //         if (currentScale != m_Scale.vector3Value)
            //             m_Scale.vector3Value = currentScale;
            //     }
            // }
            // else
            {
                // EditorGUILayout.PropertyField(m_Scale, s_Contents.scaleContent);
            }
        }
    }
}