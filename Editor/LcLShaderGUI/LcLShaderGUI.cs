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
            // List<FoldoutNode> children = new List<FoldoutNode>();
            public string foldoutName;
            public bool foldoutState;
            public bool IsFoldoutHeader => pos == FoldoutPosition.Start;
            public MaterialProperty property;
            public FoldoutPosition pos = FoldoutPosition.None;
            public int indentLevel;

            public FoldoutNode(MaterialProperty property, FoldoutPosition pos)
            {
                this.property = property;
                this.pos = pos;
                // children = new List<FoldoutNode>();
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
                foldoutState = name.Equals(string.Empty) ? true : IsDisplayProp(name);
            }

            public bool GetTopState()
            {
                if (parent == null)
                {
                    return foldoutState;
                }
                else
                {
                    if (parent.foldoutState == false)
                    {
                        return false;
                    }
                    return parent.GetTopState();
                }
            }

            public bool IsDisplay()
            {
                if (IsFoldoutHeader)
                {
                    if (parent == null)
                    {
                        return true;
                    }
                    else
                    {
                        return GetTopState();
                    }
                }
                else
                {
                    if (parent == null)
                    {
                        return foldoutState;
                    }
                    else
                    {
                        return parent.IsDisplay() && foldoutState;
                    }
                }

            }
        }

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


        static bool IsDisplayProp(string propName)
        {
            float foldoutValue = m_SerializedObject.GetHiddenPropertyFloat(propName);
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
                    if (className == "Foldout")
                    {
                        pos = FoldoutPosition.Start;
                    }
                    else if (className == "FoldoutEnd")
                    {
                        pos = FoldoutPosition.End;
                    }
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
                    var parent = m_FoldoutStack.Pop();
                    node.SyncState(parent);
                    node.parent = parent;
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

        public void DrawPropertiesDefaultGUI(MaterialEditor materialEditor)
        {

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
            foreach (var node in m_FoldoutNodeList)
            {
                if (node.pos == FoldoutPosition.Start)
                {
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
                if (node.pos == FoldoutPosition.End)
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
}
