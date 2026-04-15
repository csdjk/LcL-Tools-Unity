using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LcLTools
{
    /// <summary>
    /// 挂在预制体预览根节点上，驱动所有子粒子系统在 Editor 模式下持续播放。
    /// 开关 playInEditor 打开时模拟运行；关闭时粒子静止保持当前状态。
    /// 场景切换后组件 OnEnable 会自动重新注册驱动，无需手动操作。
    /// </summary>
    [ExecuteAlways]
    [AddComponentMenu("")] // 不出现在 Add Component 菜单，由工具自动挂载
    public class EffectPreviewPlayer : MonoBehaviour
    {
        [Tooltip("开启后在 Editor 模式下自动驱动子粒子系统播放（不影响运行时行为）")]
        public bool playInEditor = true;

        // 只驱动「根」PS（withChildren=true 会自动递归子PS）
        private ParticleSystem[] m_RootPS;

#if UNITY_EDITOR
        private double m_LastTime;

        private void OnEnable()
        {
            if (Application.isPlaying) return;
            CollectRootPS();
            PlayAll();
            m_LastTime = EditorApplication.timeSinceStartup;
            EditorApplication.update += EditorUpdate;
        }

        private void OnDisable()
        {
            if (Application.isPlaying) return;
            EditorApplication.update -= EditorUpdate;
        }

        private void OnValidate()
        {
            // Inspector 里切换开关时刷新场景视图
            SceneView.RepaintAll();
        }

        private void EditorUpdate()
        {
            if (Application.isPlaying)
            {
                // 进入 Play Mode 后注销（Play Mode 粒子由引擎自驱）
                EditorApplication.update -= EditorUpdate;
                return;
            }

            if (!playInEditor) return;

            // 过滤已被销毁的引用
            if (m_RootPS == null) { CollectRootPS(); PlayAll(); }

            double now = EditorApplication.timeSinceStartup;
            float  dt  = (float)(now - m_LastTime);
            m_LastTime = now;

            // 跳过异常大帧（窗口失焦 / 断点续帧）
            if (dt <= 0f || dt > 0.5f) return;

            foreach (var ps in m_RootPS)
            {
                if (ps == null) continue;
                ps.Simulate(dt, withChildren: true, restart: false, fixedTimeStep: false);
            }

            SceneView.RepaintAll();
        }
#endif

        // ── 工具方法（Editor + Runtime 均可调用）─────────────────
        /// <summary>收集挂载对象下所有「根」ParticleSystem</summary>
        public void CollectRootPS()
        {
            var all = GetComponentsInChildren<ParticleSystem>(true);
            var list = new List<ParticleSystem>();
            foreach (var ps in all)
            {
                // 「根」判断：父链上没有其他 ParticleSystem
                bool isRoot = ps.transform.parent == null
                    || ps.transform.parent.GetComponentInParent<ParticleSystem>() == null;
                if (isRoot) list.Add(ps);
            }
            m_RootPS = list.ToArray();
        }

        /// <summary>对所有子 ParticleSystem 调用 Play（不递归，withChildren 由 Simulate 负责）</summary>
        public void PlayAll()
        {
            var all = GetComponentsInChildren<ParticleSystem>(true);
            foreach (var ps in all)
                ps.Play(false);
        }
    }
}
