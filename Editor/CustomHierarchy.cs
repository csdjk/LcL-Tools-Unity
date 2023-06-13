using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using UnityEngine.AI;
using System.Reflection;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LcLTools
{

    [InitializeOnLoad]
    public class CustomHierarchy
    {
        // 总的开关用于开启或关闭
        private static readonly bool EnableCustomHierarchy = true;
        enum ESetOp
        {
            None,
            Self,
            Hierarchy,
        }
        private static readonly StaticEditorFlags StaticNothing = (StaticEditorFlags)0;
        private static readonly StaticEditorFlags StaticEverything;
        private const int RectWidth = 18;
        private delegate void DisplayCustomMaskMenu(Rect rect, string[] optionArray, int[] selectArray, EditorUtility.SelectMenuItemFunction callBack, object userData);
        private static DisplayCustomMaskMenu m_displayCustomMaskMenu;
        private static string[] m_optionArray =
        {
        "Nothing",
        "Everything",
        "LightmapStatic",
        "OccluderStatic",
        "OccludeeStatic",
        "BatchingStatic",
        "NavigationStatic",
        "OffMeshLinkGeneration",
        "ReflectionProbeStatic",
    };

        private static StaticEditorFlags[] m_flagsArray =
        {
        StaticNothing,
        StaticEverything,
        StaticEditorFlags.ContributeGI,
        StaticEditorFlags.OccluderStatic,
        StaticEditorFlags.OccludeeStatic,
        StaticEditorFlags.BatchingStatic,
        StaticEditorFlags.NavigationStatic,
        StaticEditorFlags.OffMeshLinkGeneration,
        StaticEditorFlags.ReflectionProbeStatic,
    };

        static CustomHierarchy()
        {
            StaticEverything = 0;
            var values = Enum.GetValues(typeof(StaticEditorFlags));
            foreach (var upper in values)
            {
                StaticEverything |= (StaticEditorFlags)upper;
            }
            MethodInfo info = typeof(EditorUtility).GetMethod(
                "DisplayCustomMenu",
                BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic,
                null,
                new[]
                {
                typeof(Rect),
                typeof(string[]),
                typeof(int[]),
                typeof(EditorUtility.SelectMenuItemFunction),
                typeof(object)
                },
                null);
            if (info != null)
            {
                m_displayCustomMaskMenu = Delegate.CreateDelegate(typeof(DisplayCustomMaskMenu), info) as DisplayCustomMaskMenu;
            }

            EditorApplication.hierarchyWindowItemOnGUI -= HierarchyWindowOnGui;
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowOnGui;
        }

        // 绘制Rect
        private static Rect CreateRect(Rect selectionRect, int index)
        {
            var rect = new Rect(selectionRect);
            rect.x += rect.width - 20 - (20 * index);
            rect.width = RectWidth;
            return rect;
        }

        private static List<int> m_selectedList = new List<int>();
        private static void HierarchyWindowOnGui(int instanceId, Rect selectionRect)
        {
            if (!EnableCustomHierarchy) return;
            var rect = new Rect(selectionRect);
            rect.x += rect.width - 30;
            rect.width = RectWidth;

            var go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
            if (!go) return;

            bool active = go.activeSelf;
            if (EditorGUI.Toggle(rect, active) != active)
            {
                go.SetActive(!active);
            }

            // 图标的序列号
            var index = 0;
            GUIStyle style = null;
            SetColor(rect, go, ref index, ref style);

            // is Static 
            rect = new Rect(selectionRect);
            rect.x += rect.width;
            rect.width = RectWidth;
            if (go.isStatic)
            {
                GUI.Label(rect, "S");
            }

            if (m_displayCustomMaskMenu == null) return;

            rect.x -= 10;
            m_selectedList.Clear();
            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(go);
            if ((flags & StaticEverything) == 0)
                m_selectedList.Add(0);
            else
            {
                for (int i = 1; i < m_flagsArray.Length; ++i)
                {
                    var temp = m_flagsArray[i];
                    if ((flags & temp) == temp)
                        m_selectedList.Add(i);
                }
            }

            string ft = (flags & StaticEverything) == 0 ? "□" : ((flags & StaticEverything) == StaticEverything ? "■" : "▂");
            if (GUI.Button(rect, ft, EditorStyles.label))
            {
                m_displayCustomMaskMenu(
                    rect,
                    m_optionArray,
                    m_selectedList.ToArray(),
                    (u, optionList, selected) =>
                    {
                        bool add;
                        if (selected == 0)
                            add = false;
                        else if (selected == 1)
                            add = true;
                        else
                            add = (flags & m_flagsArray[selected]) != m_flagsArray[selected];
                        ESetOp setOp = ESetOp.Self;

                        if (go.transform.childCount > 0)
                        {
                            string str;
                            if (selected == 0)
                                str = "disable the";
                            else if (selected == 1)
                                str = "enable the";
                            else if (add)
                                str = "enable the " + m_flagsArray[selected].ToString();
                            else
                                str = "disable the " + m_flagsArray[selected].ToString();

                            int op = EditorUtility.DisplayDialogComplex(
                                "Change Static Flags",
                                "Do you want to " + str + " static flags for all the child objects as well?",
                                "Yes, change children",
                                "No, this object only",
                                "Cancel");
                            switch (op)
                            {
                                case 0:
                                    setOp = ESetOp.Hierarchy;
                                    break;
                                case 1:
                                    setOp = ESetOp.Self;
                                    break;
                                case 2:
                                    setOp = ESetOp.None;
                                    break;
                            }
                        }

                        switch (setOp)
                        {
                            case ESetOp.Self:
                                {
                                    if (selected == 0)
                                        flags = StaticNothing;
                                    else if (selected == 1)
                                        flags = StaticEverything;
                                    else
                                    {
                                        if ((flags & m_flagsArray[selected]) == m_flagsArray[selected])
                                            flags = flags & ~m_flagsArray[selected];
                                        else
                                            flags = flags | m_flagsArray[selected];
                                    }

                                    GameObjectUtility.SetStaticEditorFlags(go, flags);
                                }
                                break;
                            case ESetOp.Hierarchy:
                                {
                                    if (selected == 0)
                                        SetStaticEditorFlagsHierarchy(go.transform, StaticNothing);
                                    else if (selected == 1)
                                        SetStaticEditorFlagsHierarchy(go.transform, StaticEverything);
                                    else
                                    {
                                        if (add)
                                            AddStaticEditorFlagsHierarchy(go.transform, m_flagsArray[selected]);
                                        else
                                            RemoveStaticEditorFlagsHierarchy(go.transform,
                                                    m_flagsArray[selected]);
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                    }, null);
            }
        }

        private static void SetColor(Rect rect, GameObject go, ref int index, ref GUIStyle style)
        {
            DrawRectIcon<MeshRenderer>(rect, go, ref index, ref style);
            DrawRectIcon<SkinnedMeshRenderer>(rect, go, ref index, ref style);
            DrawRectIcon<BoxCollider>(rect, go, ref index, ref style);
            DrawRectIcon<SphereCollider>(rect, go, ref index, ref style);
            DrawRectIcon<CapsuleCollider>(rect, go, ref index, ref style);
            DrawRectIcon<MeshCollider>(rect, go, ref index, ref style);
            DrawRectIcon<CharacterController>(rect, go, ref index, ref style);
            DrawRectIcon<Rigidbody>(rect, go, ref index, ref style);
            DrawRectIcon<Light>(rect, go, ref index, ref style);
            DrawRectIcon<Animator>(rect, go, ref index, ref style);
            DrawRectIcon<Animation>(rect, go, ref index, ref style);
            DrawRectIcon<Camera>(rect, go, ref index, ref style);
            DrawRectIcon<Projector>(rect, go, ref index, ref style);
            DrawRectIcon<NavMeshAgent>(rect, go, ref index, ref style);
            DrawRectIcon<NavMeshObstacle>(rect, go, ref index, ref style);
            DrawRectIcon<ParticleSystem>(rect, go, ref index, ref style);
            DrawRectIcon<AudioSource>(rect, go, ref index, ref style);
            DrawRectIcon<Image>(rect, go, ref index, ref style);
            DrawRectIcon<Button>(rect, go, ref index, ref style);
            DrawRectIcon<Toggle>(rect, go, ref index, ref style);
            DrawRectIcon<Text>(rect, go, ref index, ref style);
        }

        //随便找个比较直观顺眼的图标
        private static Texture m_raycastIcon
        {
            get { return EditorGUIUtility.ObjectContent(null, typeof(PhysicsRaycaster)).image; }
        }

        private static void DrawRectIcon<T>(Rect selectionRect, GameObject go, ref int index, ref GUIStyle style) where T : Component
        {
            var c = go.GetComponent<T>();
            if (c)
            {
                // 图标的绘制排序
                var rect = CreateRect(selectionRect, ++index);

                // 绘制图标
                var icon = EditorGUIUtility.ObjectContent(null, typeof(T)).image;
                GUI.Label(rect, icon);

                //raycastTarget选中检测
                var mg = c as MaskableGraphic;
                if (mg && mg.raycastTarget)
                {
                    rect = CreateRect(selectionRect, ++index);
                    if (GUI.Button(rect, m_raycastIcon, EditorStyles.label))
                    {
                        mg.raycastTarget = false;
                    }
                }
            }
        }

        static void SetStaticEditorFlagsHierarchy(Transform transform, StaticEditorFlags flags)
        {
            GameObjectUtility.SetStaticEditorFlags(transform.gameObject, flags);
            foreach (Transform child in transform)
            {
                SetStaticEditorFlagsHierarchy(child, flags);
            }
        }

        static void AddStaticEditorFlagsHierarchy(Transform transform, StaticEditorFlags flags)
        {
            GameObjectUtility.SetStaticEditorFlags(transform.gameObject, GameObjectUtility.GetStaticEditorFlags(transform.gameObject) | flags);
            foreach (Transform child in transform)
            {
                AddStaticEditorFlagsHierarchy(child, flags);
            }
        }

        static void RemoveStaticEditorFlagsHierarchy(Transform transform, StaticEditorFlags flags)
        {
            GameObjectUtility.SetStaticEditorFlags(transform.gameObject, GameObjectUtility.GetStaticEditorFlags(transform.gameObject) & ~flags);
            foreach (Transform child in transform)
            {
                RemoveStaticEditorFlagsHierarchy(child, flags);
            }
        }
    }
}