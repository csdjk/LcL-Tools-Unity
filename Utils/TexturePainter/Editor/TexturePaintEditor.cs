// using System.IO;
// using UnityEngine;
// using UnityEditor;
// using UnityEditor.SceneManagement;
// using UnityEngine.SceneManagement;
// using System;
// namespace LcLTools
// {
//     [CustomEditor(typeof(TexturePainter))]
//     public class TexturePainterEditor : Editor
//     {
//         TexturePainter m_Painter;
//         public Texture2D targetTexture;
//         private Color m_BrushColor = Color.red;
//         private float m_BrushSizeMax = 5f;
//         private float m_BrushSize = 0.5f;
//         private float m_BrushStrengthMax = 2f;
//         private float m_BrushStrength = 1f;
//         private float m_BrushHardness = 0.5f;
//
//
//         private Event m_CurrentEvent;
//         private const float MOUSE_WHEEL_BRUSH_SIZE_MULTIPLIER = 0.01f;
//
//         GUIStyle m_TargetLabelStyle = new GUIStyle();
//         int m_SelectIndex;
//
//         void OnEnable()
//         {
//             m_Painter = target as TexturePainter;
//             m_SelectIndex = m_Painter.TargetIndex;
//             m_TargetLabelStyle.alignment = TextAnchor.MiddleCenter;
//             m_TargetLabelStyle.normal.textColor = new Color(0, 0.75f, 0);
//         }
//
//         // 预览面板
//         public override bool HasPreviewGUI()
//         {
//             return true;
//         }
//         public override void OnPreviewGUI(Rect r, GUIStyle background)
//         {
//             m_Painter = target as TexturePainter;
//             GUI.DrawTexture(r, m_Painter.CurrentTexture, ScaleMode.ScaleToFit, false);
//
//
//             // float halfWidth = r.width / 2;
//             // float size = Mathf.Min(halfWidth, r.height);
//             // // Texture1
//             // float rectx = r.x + halfWidth / 2 - size / 2;
//             // float recty = r.y + r.height / 2 - size / 2;
//             // Rect rect = new Rect(rectx, recty, size, size);
//             // GUI.DrawTexture(rect, painter.currentTexture, ScaleMode.ScaleToFit, false);
//
//             // // Texture2
//             // float rectx2 = r.x + halfWidth + halfWidth / 2 - size / 2;
//             // float recty2 = r.y + r.height / 2 - size / 2;
//             // Rect rect2 = new Rect(rectx2, recty2, size, size);
//             // GUI.DrawTexture(rect2, painter.compositeTexture, ScaleMode.ScaleToFit, true);
//         }
//
//         // Inspector
//         public override void OnInspectorGUI()
//         {
//             m_Painter = target as TexturePainter;
//             DrawBrushInfoGUI();
//             DrawButtonGUI();
//         }
//
//         bool m_IsDraw = false;
//         BlendModel m_BlendModel;
//         private bool m_IsShiftDown = false;
//         void OnSceneGUI()
//         {
//             m_CurrentEvent = Event.current;
//             m_Painter = target as TexturePainter;
//             if (m_Painter == null)
//                 return;
//             if (!m_Painter.isActiveAndEnabled)
//                 return;
//
//             m_Painter.HideUnityTools();
//
//             HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Keyboard));
//             Vector2 mousePosition = Event.current.mousePosition;
//             Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
//
//             RaycastHit hit;
//             if (Physics.Raycast(ray, out hit))
//             {
//                 // 判断碰撞物体是否为当前绘制对象
//                 // if (hit.collider.gameObject != painter.gameObject)
//                 // {
//                 //     return;
//                 // }
//                 EditorApplication.QueuePlayerLoopUpdate();
//
//                 HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
//                 m_Painter.SetBrushInfo(m_BrushSize, m_BrushStrength, m_BrushHardness, m_BrushColor);
//                 if (m_Painter.drawModel == DrawModel.BaseWorldPos)
//                 {
//                     m_Painter.SetMousePos(hit.point);
//                 }
//                 else
//                 {
//                     m_Painter.SetMousePos(hit.textureCoord);
//                 }
//
//                 // Debug.Log("hit:" + hit.point);
//                 // painter.DrawAt();
//                 // painter.SetMouseState(true);
//
//
//                 // 第一次按下shift键时，记录blendModel
//                 if (m_CurrentEvent.shift && !m_IsShiftDown)
//                 {
//                     m_BlendModel = m_Painter.BlendModel;
//                     if (m_BlendModel == BlendModel.Add)
//                         m_Painter.BlendModel = BlendModel.Sub;
//                     else
//                         m_Painter.BlendModel = BlendModel.Add;
//
//                     m_IsShiftDown = true;
//                 }
//                 else if (!m_CurrentEvent.shift && m_IsShiftDown)
//                 {
//                     m_IsShiftDown = false;
//                     m_Painter.BlendModel = m_BlendModel;
//                 }
//
//
//                 // if ((currentEvent.type == EventType.MouseDrag || currentEvent.type == EventType.MouseDown) && Event.current.button == 0)
//                 // {
//                 //     isDraw = true;
//                 // }
//                 // if (currentEvent.type == EventType.MouseUp && Event.current.button == 0)
//                 // {
//                 //     isDraw = false;
//                 // }
//
//                 // if (isDraw)
//                 // {
//                 //     painter.DrawAt();
//                 // }
//                 // painter.SetMouseState(isDraw);
//
//
//
//                 if ((m_CurrentEvent.type == EventType.MouseDrag || m_CurrentEvent.type == EventType.MouseDown) && Event.current.button == 0)
//                 {
//                     m_Painter.DrawAt();
//                     m_Painter.SetMouseState(true);
//                 }
//                 else
//                 {
//                     m_Painter.SetMouseState(false);
//                 }
//
//
//
//                 // shift + 鼠标滚轮调整brushSize
//                 if (m_CurrentEvent.type == EventType.ScrollWheel && m_CurrentEvent.shift)
//                 {
//                     m_BrushSize -= m_CurrentEvent.delta.y * MOUSE_WHEEL_BRUSH_SIZE_MULTIPLIER;
//                     m_BrushSize = Mathf.Clamp(m_BrushSize, 0, m_BrushSizeMax);
//                     m_CurrentEvent.Use();
//                 }
//                 else if (m_CurrentEvent.type == EventType.ScrollWheel && m_CurrentEvent.control)
//                 {
//                     m_BrushHardness += m_CurrentEvent.delta.y * MOUSE_WHEEL_BRUSH_SIZE_MULTIPLIER;
//                     m_BrushHardness = Mathf.Clamp(m_BrushHardness, 0, 1);
//                     m_CurrentEvent.Use();
//                 }
//                 else if (m_CurrentEvent.type == EventType.ScrollWheel && m_CurrentEvent.alt)
//                 {
//                     m_BrushStrength += m_CurrentEvent.delta.y * 0.2f;
//                     m_BrushStrength = Mathf.Clamp(m_BrushStrength, 0, m_BrushStrengthMax);
//                     m_CurrentEvent.Use();
//                 }
//
//
//                 SceneView.lastActiveSceneView.camera.Render();
//
//                 Handles.color = Color.red;
//                 Vector3 endPt = hit.point + (Vector3.Normalize(hit.normal) * m_BrushSize);
//                 Handles.DrawAAPolyLine(2f, new Vector3[] { hit.point, endPt });
//                 Handles.CircleHandleCap(0, hit.point, Quaternion.FromToRotation(Vector3.forward, hit.normal), m_BrushSize, EventType.Repaint);
//
//             }
//             DrawTipsInfo();
//
//
//             Handles.BeginGUI();
//             {
//                 var size = 256;
//                 Rect rect = new Rect(0, Screen.height - size - 50, size, size);
//                 GUI.DrawTexture(rect, m_Painter.CurrentTexture, ScaleMode.ScaleToFit, false);
//             }
//             Handles.EndGUI();
//
//             SceneView.RepaintAll();
//         }
//
//
//         string m_KeywordTips = "取反笔刷模式：Shift + 鼠标左键 \n调整笔刷大小：Shift + 鼠标中键滚轮  调整笔刷硬度：Ctrl + 鼠标中键滚轮";
//
//         public void DrawTipsInfo()
//         {
//             float windowWidth = Screen.width;
//             float windowHeight = Screen.height;
//             float panelWidth = 500;
//             float panelHeight = 100;
//             float panelX = windowWidth * 0.5f - panelWidth * 0.5f;
//             float panelY = windowHeight - panelHeight;
//
//             GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel);
//             labelStyle.alignment = TextAnchor.MiddleCenter;
//             labelStyle.normal.textColor = Color.white;
//             // 快捷键提示
//             Rect infoRect = new Rect(panelX, panelY, panelWidth, panelHeight);
//
//             GUILayout.BeginArea(infoRect, GUI.skin.box);
//             {
//
//                 EditorGUILayout.BeginVertical();
//                 {
//                     GUILayout.Label(m_KeywordTips, labelStyle);
//                     GUILayout.Label($"Mouse Pos{m_Painter.mousePos}", labelStyle);
//                 }
//                 EditorGUILayout.EndVertical();
//             }
//             GUILayout.EndArea();
//         }
//
//
//
//         public void DrawButtonGUI()
//         {
//             EditorGUILayout.BeginHorizontal();
//             {
//                 if (GUILayout.Button("另存为"))
//                 {
//                     m_Painter.SaveAs();
//                 }
//                 if (GUILayout.Button("保存纹理"))
//                 {
//                     m_Painter.Save();
//                 }
//             }
//             EditorGUILayout.EndHorizontal();
//
//             EditorGUILayout.BeginHorizontal();
//             {
//
//                 if (GUILayout.Button("重置"))
//                 {
//                     if (EditorUtility.DisplayDialog("Clear", "该操作会清空当前所绘制内容!", "OK", "Cancel"))
//                     {
//                         m_Painter.ClearPaint();
//                         m_Painter.InitData();
//                         m_Painter.ChangeTarget();
//                     }
//                 }
//                 if (GUILayout.Button("删除脚本"))
//                 {
//                     Undo.DestroyObjectImmediate(m_Painter);
//                 }
//             }
//             EditorGUILayout.EndHorizontal();
//         }
//
//
//
//         public void DrawBrushInfoGUI()
//         {
//             EditorGUILayout.HelpBox("绘制完成后需要点击保存,并且删除脚本！", MessageType.Warning);
//
//             if (m_Painter.textureList != null)
//             {
//                 // 绘制纹理列表
//                 var currentViewWidth = EditorGUIUtility.currentViewWidth;
//                 var boxWidth = currentViewWidth - 50;
//                 var imageWidth = 50;
//                 var xCount = Mathf.FloorToInt(boxWidth / imageWidth);
//                 var boxHeight = Mathf.CeilToInt((float)m_Painter.textureList.Length / xCount) * imageWidth;
//
//                 GUILayout.BeginHorizontal("box");
//                 {
//                     var index = m_SelectIndex;
//                     m_SelectIndex = GUILayout.SelectionGrid(m_SelectIndex, m_Painter.textureList, xCount, GUILayout.Width(boxWidth), GUILayout.Height(boxHeight));
//
//                     if (index != m_SelectIndex)
//                     {
//                         // Save();
//                         m_Painter.TargetIndex = m_SelectIndex;
//                     }
//
//                 }
//                 GUILayout.EndHorizontal();
//             }
//
//             GUILayout.Label("当前绘制对象：" + m_Painter.target?.texture?.name, m_TargetLabelStyle ?? EditorStyles.boldLabel);
//
//             m_Painter.drawModel = (DrawModel)EditorGUILayout.EnumPopup("模式", m_Painter.drawModel);
//
//
//             m_Painter.texcoordChannel = (TexCoordChannel)EditorGUILayout.EnumPopup("UV", m_Painter.texcoordChannel);
//
//
//             PainterDrawChannel drawChannel = (PainterDrawChannel)EditorGUILayout.EnumPopup("绘制通道", m_Painter.drawChannel);
//             if (m_Painter.drawChannel != drawChannel)
//             {
//                 m_Painter.drawChannel = drawChannel;
//             }
//             BlendModel blendModel = (BlendModel)EditorGUILayout.EnumPopup("绘制模式", m_Painter.BlendModel);
//             if (m_Painter.BlendModel != blendModel)
//             {
//                 m_Painter.BlendModel = blendModel;
//             }
//             if (m_Painter.drawChannel == PainterDrawChannel.RGB || m_Painter.drawChannel == PainterDrawChannel.RGBA)
//             {
//                 m_BrushColor = EditorGUILayout.ColorField("笔刷颜色", m_BrushColor);
//             }
//             m_BrushSize = EditorGUILayout.Slider("笔刷大小", m_BrushSize, 0, m_BrushSizeMax);
//             m_BrushStrength = EditorGUILayout.Slider("笔刷强度", m_BrushStrength, 0, m_BrushStrengthMax);
//             m_BrushHardness = EditorGUILayout.Slider("笔刷硬度", m_BrushHardness, 0, 1);
//         }
//
//         [MenuItem("GameObject/Add Texture Painter", false, 0)]
//         static void AddTexturePainter()
//         {
//             var selectGo = Selection.activeGameObject;
//             if (selectGo)
//             {
//                 if (selectGo.GetComponent<TexturePainter>())
//                 {
//                     Debug.LogWarning("该对象已有Painter组件!!!");
//                 }
//                 else
//                 {
//                     selectGo.AddComponent<TexturePainter>();
//                 }
//             }
//         }
//
//
//     }
// }
