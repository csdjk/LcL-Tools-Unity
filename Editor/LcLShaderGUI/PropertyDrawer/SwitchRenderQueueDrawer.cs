using UnityEngine;
using UnityEditor;
using System;

namespace LcLShaderEditor
{
    /// <summary>
    /// 根据keyword切换RenderQueue
    /// </summary>
    public class SwitchRenderQueueDrawer : MaterialPropertyDrawer
    {
        string m_Keyword;
        int m_RenderQueue1;
        int m_RenderQueue2;

        public SwitchRenderQueueDrawer(string keyword, float renderQueue1, float renderQueue2)
        {
            this.m_Keyword = keyword;
            this.m_RenderQueue1 = (int)renderQueue1;
            this.m_RenderQueue2 = (int)renderQueue2;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            var mat = prop.targets[0] as Material;
            if (mat.IsKeywordEnabled(m_Keyword))
            {
                mat.renderQueue = m_RenderQueue1;
            }
            else
            {
                mat.renderQueue = m_RenderQueue2;
            }
        }
    }
}
