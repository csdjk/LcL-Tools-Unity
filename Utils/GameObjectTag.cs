using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LcLTools
{
    public class GameObjectTag : MonoBehaviour
    {
        public string tagName;

        public float height = 20;
        public int fontSize = 20;
        public Color color = Color.white;

#if UNITY_EDITOR
        private static GUIStyle style;

        private static GUIStyle Style
        {
            get
            {
                if (style == null)
                {
                    style = new GUIStyle(EditorStyles.label);
                    style.alignment = TextAnchor.MiddleCenter;
                    style.normal.textColor = new Color(1, 1, 1, 0.8f);
                }

                return style;
            }
        }


        [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
        static void DrawGizmo(GameObjectTag goTag, GizmoType gizmoType)
        {
            var transform = goTag.transform;
            var position = transform.position;
            var height = goTag.height;
            Style.fontSize = goTag.fontSize;

            var mesh = goTag.transform.GetComponentInChildren<MeshRenderer>();
            if (mesh)
            {
                position = mesh.bounds.center;
            }

            var tagName = goTag.tagName.Equals(String.Empty) ? goTag.name : goTag.tagName;
            var labelPosition = position + Vector3.up * height;


            //世界坐标转屏幕坐标
            CameraProjectionCache cam = new CameraProjectionCache(Camera.current);
            Vector2 screenPosition = cam.WorldToGUIPoint(labelPosition);

            Vector2 stringSize = Style.CalcSize(new GUIContent(tagName));
            Rect rect = new Rect(0f, 0f, stringSize.x + 6, stringSize.y + 4);
            rect.center = screenPosition;

            Handles.BeginGUI();
            {
                GUI.color = new Color(0, 0, 0, 0.5f);
                GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);
                GUI.color = goTag.color;
                GUI.Label(rect, tagName, Style);
            }
            Handles.EndGUI();

            // Gizmos.DrawLine(position, labelPosition);
        }
#endif
    }
}
