using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.Text.RegularExpressions;
using TMPro.EditorUtilities;
using System.Linq;
using LcLShaderEditor;

namespace LcLShaderEditor
{
    public class LcLShaderGUI : ShaderGUI
    {
        private const string k_FoldoutClassName = "Foldout";
        private const string k_FoldoutEndClassName = "FoldoutEnd";

        static Material m_CopiedProperties;
        static SerializedObject m_SerializedObject;
        static Stack<FoldoutNode> m_FoldoutStack = new Stack<FoldoutNode>();
        static List<FoldoutNode> m_FoldoutNodeList = new List<FoldoutNode>();

        static void GetAttr(string attr, out string className, out string args)
        {
            Match match = Regex.Match(attr, @"(\w+)\s*\((.*)\)");
            if (match.Success)
            {
                className = match.Groups[1].Value.Trim();
                args = match.Groups[2].Value.Trim();
            }
            else
            {
                className = attr;
                args = "";
            }
        }

        public static bool IsDisplayProp(string propName)
        {
            var foldoutValue = m_SerializedObject.GetProperty(propName).GetPropertyIntValue();
            return foldoutValue > 0;
        }


        override public void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            m_FoldoutStack.Clear();
            m_FoldoutNodeList.Clear();
            var material = materialEditor.target as Material;
            m_SerializedObject = new SerializedObject(material);

            InitNodeList(properties, material);
            DrawPropertiesDefaultGUI(materialEditor);
            DrawPropertiesContextMenu(materialEditor);

            m_SerializedObject.Dispose();
        }

        public static void InitNodeList(MaterialProperty[] properties, Material material)
        {
            Shader shader = material.shader;
            for (int i = 0; i < properties.Length; i++)
            {
                var prop = properties[i];
                FoldoutPosition pos = FoldoutPosition.Middle;
                string[] attributes = shader.GetPropertyAttributes(i);

                foreach (var attr in attributes)
                {
                    GetAttr(attr, out var className, out var args);
                    if (className.Equals(k_FoldoutClassName))
                        pos = FoldoutPosition.Start;
                    else if (className.Equals(k_FoldoutEndClassName))
                        pos = FoldoutPosition.End;
                }

                var node = new FoldoutNode(prop, pos)
                {
                    indentLevel = m_FoldoutStack.Count
                };

                if (pos == FoldoutPosition.Start)
                {
                    node.SetFoldoutName(ShaderEditorHandler.GetFoldoutPropName(prop.name));
                    node.parent = m_FoldoutStack.TryPeek(out var parent) ? parent : null;
                    m_FoldoutStack.Push(node);
                }
                else if (pos == FoldoutPosition.End)
                {
                    if (m_FoldoutStack.Count > 0)
                    {
                        var parent = m_FoldoutStack.Pop();
                        node.SyncState(parent);
                        node.parent = parent;
                    }
                    else
                        Debug.LogWarning("FoldoutEnd found without matching FoldoutStart");
                }
                else
                {
                    if (m_FoldoutStack.Count > 0)
                    {
                        var parent = m_FoldoutStack.Peek();
                        node.SyncState(parent);
                        node.parent = parent;
                    }
                    else
                    {
                        node.SetFoldoutName("");
                    }
                }
                m_FoldoutNodeList.Add(node);
            }
        }

        private static int m_ControlHash = "EditorTextField".GetHashCode();
        int m_FoldoutMatchCount = 0;

        public void DrawPropertiesDefaultGUI(MaterialEditor materialEditor)
        {
            //检测foldout的Start和End的个数是否匹配
            if (m_FoldoutMatchCount != 0)
            {
                var flag = "End";
                if (m_FoldoutMatchCount < 0)
                {
                    for (int i = 0; i < -m_FoldoutMatchCount; i++)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    }

                    flag = "Start";
                }

                //绘制提示信息
                EditorGUILayout.HelpBox($"Foldout的Start和End个数不匹配,缺少了{Mathf.Abs(m_FoldoutMatchCount)}个{flag},已自动补充,GUI面板层级有可能不符合预期,请检查Properties!", MessageType.Error);
            }


            var f = materialEditor.GetType().GetField("m_InfoMessage", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (f != null)
            {
                string m_InfoMessage = (string)f.GetValue(materialEditor);
                materialEditor.SetDefaultGUIWidths();
                if (m_InfoMessage != null)
                {
                    EditorGUILayout.HelpBox(m_InfoMessage, MessageType.Info);
                }
                else
                {
                    GUIUtility.GetControlID(m_ControlHash, FocusType.Passive, new Rect(0f, 0f, 0f, 0f));
                }
            }

            m_FoldoutMatchCount = 0;
            foreach (var node in m_FoldoutNodeList)
            {
                if (node.IsFoldoutHeader)
                {
                    m_FoldoutMatchCount++;
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                }

                var display = node.IsDisplay();
                if (display)
                {
                    EditorGUI.indentLevel += node.indentLevel;
                    if ((node.property.flags & (MaterialProperty.PropFlags.HideInInspector | MaterialProperty.PropFlags.PerRendererData)) == MaterialProperty.PropFlags.None)
                    {
                        float propertyHeight = materialEditor.GetPropertyHeight(node.property, node.property.displayName);
                        Rect controlRect = EditorGUILayout.GetControlRect(true, propertyHeight, EditorStyles.layerMaskField);
                        materialEditor.ShaderProperty(controlRect, node.property, node.property.displayName);
                    }

                    EditorGUI.indentLevel -= node.indentLevel;
                }

                if (node.IsFoldoutEnd)
                {
                    m_FoldoutMatchCount--;
                    EditorGUILayout.EndVertical();
                }
            }

            if (m_FoldoutMatchCount > 0)
            {
                for (int i = 0; i < m_FoldoutMatchCount; i++)
                {
                    EditorGUILayout.EndVertical();
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            if (SupportedRenderingFeatures.active.editableMaterialRenderQueue)
            {
                materialEditor.RenderQueueField();
            }

            materialEditor.EnableInstancingField();
            materialEditor.DoubleSidedGIField();
        }

        public static void DrawPropertiesContextMenu(MaterialEditor materialEditor)
        {
            if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
            {
                var material = materialEditor.target as Material;

                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Copy"), false, () => Copy(material));
                menu.AddItem(new GUIContent("Paste"), false, () => Paste(material));
                menu.AddItem(new GUIContent("Reset"), false, () => Reset(material));
                menu.ShowAsContext();
            }
        }


        private static void Copy(Material material)
        {
            m_CopiedProperties = new Material(material)
            {
                shaderKeywords = material.shaderKeywords,
                hideFlags = HideFlags.DontSave
            };
        }

        private static void Paste(Material material)
        {
            if (m_CopiedProperties == null)
                return;

            Undo.RecordObject(material, "Paste Material");

            EditorShaderUtilities.CopyMaterialProperties(m_CopiedProperties, material);
            material.shaderKeywords = m_CopiedProperties.shaderKeywords;
        }

        private static void Reset(Material material)
        {
            Undo.RecordObject(material, "Reset Material");

            // Reset the material
            Unsupported.SmartReset(material);
            // Reset ShaderKeywords
            material.shaderKeywords = new string[0];
        }
    }

    public enum FoldoutPosition
    {
        Start,
        Middle,
        End,
        None
    }

    class FoldoutNode
    {
        public FoldoutNode parent;
        public string foldoutName;
        public bool foldoutState;
        public bool IsFoldoutHeader => pos == FoldoutPosition.Start;
        public bool IsFoldoutEnd => pos == FoldoutPosition.End;
        public MaterialProperty property;
        public FoldoutPosition pos = FoldoutPosition.None;
        public int indentLevel;

        public FoldoutNode(MaterialProperty property, FoldoutPosition pos)
        {
            this.property = property;
            this.pos = pos;
        }

        public void SyncState(FoldoutNode node)
        {
            SetFoldoutState(node.foldoutState);
            foldoutName = node.foldoutName;
        }

        public void SetFoldoutState(bool value)
        {
            foldoutState = value;
        }

        public void SetFoldoutName(string name)
        {
            foldoutName = name;
            foldoutState = name.Equals(string.Empty) || LcLShaderGUI.IsDisplayProp(name);
        }

        public bool GetTopState()
        {
            if (parent == null) return foldoutState;
            else
            {
                if (parent.foldoutState == false) return false;
                return parent.GetTopState();
            }
        }

        public bool IsDisplay()
        {
            if (IsFoldoutHeader)
            {
                if (parent == null)
                    return true;
                else
                    return GetTopState();
            }
            else
            {
                if (parent == null)
                    return foldoutState;
                else
                    return parent.IsDisplay() && foldoutState;
            }
        }
    }
}