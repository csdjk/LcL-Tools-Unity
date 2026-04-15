using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LcLTools
{
    /// <summary>
    /// 特效批量预览工具
    /// 将指定目录下的所有预制体批量生成到当前场景，以 Grid 方式排列在 Y=0 高度
    /// </summary>
    public class EffectPreviewTool : EditorWindow
    {
        // ── 持久化设置 ──────────────────────────────────────────
        private const string PrefKey_FolderPath  = "LcLEffectPreview_FolderPath";
        private const string PrefKey_Columns     = "LcLEffectPreview_Columns";
        private const string PrefKey_SpacingX    = "LcLEffectPreview_SpacingX";
        private const string PrefKey_SpacingZ    = "LcLEffectPreview_SpacingZ";
        private const string PrefKey_Recursive   = "LcLEffectPreview_Recursive";
        private const string PrefKey_AutoLoop    = "LcLEffectPreview_AutoLoop";
        private const string GroupName           = "[EffectPreview]";

        // ── 窗口状态 ──────────────────────────────────────────
        private DefaultAsset m_FolderAsset = null;   // Project 窗口中的文件夹引用
        private string  m_FolderPath  = "Assets";    // 由 m_FolderAsset 派生，仅内部使用
        private int     m_Columns     = 5;
        private float   m_SpacingX    = 3f;
        private float   m_SpacingZ    = 3f;
        private bool    m_Recursive   = true;
        private bool    m_AutoLoop    = true;        // 生成时是否自动开启粒子 Loop

        private Vector2 m_ScrollPos;
        private List<string> m_FoundPrefabs = new List<string>();
        private bool m_NeedRefresh = true;

        // ── 样式 ──────────────────────────────────────────────
        private GUIStyle m_HeaderStyle;
        private GUIStyle m_BoxStyle;

        // ─────────────────────────────────────────────────────
        [MenuItem("LcLTools/Effect Preview Tool")]
        public static void Open()
        {
            var win = GetWindow<EffectPreviewTool>(false, "Effect Preview Tool");
            win.minSize = new Vector2(340, 420);
            win.Show();
        }

        // ─────────────────────────────────────────────────────
        private void OnEnable()
        {
            // 从持久化路径恢复文件夹引用
            m_FolderPath = EditorPrefs.GetString(PrefKey_FolderPath, "Assets");
            m_FolderAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(m_FolderPath);
            m_Columns    = EditorPrefs.GetInt(PrefKey_Columns, 5);
            m_SpacingX   = EditorPrefs.GetFloat(PrefKey_SpacingX, 3f);
            m_SpacingZ   = EditorPrefs.GetFloat(PrefKey_SpacingZ, 3f);
            m_Recursive  = EditorPrefs.GetBool(PrefKey_Recursive, true);
            m_AutoLoop   = EditorPrefs.GetBool(PrefKey_AutoLoop, true);
            m_NeedRefresh = true;
        }

        private void OnDisable()
        {
            SavePrefs();
        }

        private void SavePrefs()
        {
            EditorPrefs.SetString(PrefKey_FolderPath, m_FolderPath);
            EditorPrefs.SetInt(PrefKey_Columns,    m_Columns);
            EditorPrefs.SetFloat(PrefKey_SpacingX, m_SpacingX);
            EditorPrefs.SetFloat(PrefKey_SpacingZ, m_SpacingZ);
            EditorPrefs.SetBool(PrefKey_Recursive, m_Recursive);
            EditorPrefs.SetBool(PrefKey_AutoLoop,  m_AutoLoop);
        }

        // ─────────────────────────────────────────────────────
        private void InitStyles()
        {
            if (m_HeaderStyle != null) return;

            m_HeaderStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize  = 13,
                alignment = TextAnchor.MiddleLeft
            };

            m_BoxStyle = new GUIStyle("HelpBox")
            {
                padding = new RectOffset(10, 10, 8, 8)
            };
        }

        // ─────────────────────────────────────────────────────
        private void OnGUI()
        {
            InitStyles();

            m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos);

            DrawHeader();
            GUILayout.Space(6);
            DrawFolderSettings();
            GUILayout.Space(6);
            DrawGridSettings();
            GUILayout.Space(6);
            DrawActions();
            GUILayout.Space(6);
            DrawPrefabList();

            EditorGUILayout.EndScrollView();
        }

        // ── 标题 ──────────────────────────────────────────────
        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(m_BoxStyle);
            GUILayout.Label("✦ Effect Preview Tool", m_HeaderStyle);
            EditorGUILayout.LabelField("批量将预制体生成到场景并以 Grid 方式排列", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
        }

        // ── 目录设置 ──────────────────────────────────────────
        private void DrawFolderSettings()
        {
            EditorGUILayout.BeginVertical(m_BoxStyle);
            GUILayout.Label("📁 预制体目录", EditorStyles.boldLabel);
            GUILayout.Space(4);

            // 用 ObjectField 拖拽引用文件夹（DefaultAsset）
            EditorGUI.BeginChangeCheck();
            var newAsset = (DefaultAsset)EditorGUILayout.ObjectField(
                "目录", m_FolderAsset, typeof(DefaultAsset), false);
            if (EditorGUI.EndChangeCheck())
            {
                m_FolderAsset = newAsset;
                m_FolderPath  = m_FolderAsset != null
                    ? AssetDatabase.GetAssetPath(m_FolderAsset)
                    : string.Empty;
                m_NeedRefresh = true;
            }

            // 显示只读路径提示
            if (!string.IsNullOrEmpty(m_FolderPath))
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField("路径", m_FolderPath);
                EditorGUI.EndDisabledGroup();
            }

            EditorGUI.BeginChangeCheck();
            bool rec = EditorGUILayout.Toggle("包含子目录", m_Recursive);
            if (EditorGUI.EndChangeCheck())
            {
                m_Recursive   = rec;
                m_NeedRefresh = true;
            }

            if (GUILayout.Button("刷新预制体列表"))
                RefreshPrefabList();

            EditorGUILayout.EndVertical();
        }

        // ── Grid 参数 ──────────────────────────────────────────
        private void DrawGridSettings()
        {
            EditorGUILayout.BeginVertical(m_BoxStyle);
            GUILayout.Label("⚙ Grid 参数", EditorStyles.boldLabel);
            GUILayout.Space(4);

            m_Columns  = EditorGUILayout.IntSlider("列数 (Columns)", m_Columns, 1, 20);
            m_SpacingX = EditorGUILayout.Slider("X 间距", m_SpacingX, 0.5f, 20f);
            m_SpacingZ = EditorGUILayout.Slider("Z 间距", m_SpacingZ, 0.5f, 20f);

            GUILayout.Space(4);
            m_AutoLoop = EditorGUILayout.Toggle(
                new GUIContent("自动开启粒子 Loop",
                    "生成到场景时，自动将所有 ParticleSystem 的 Loop 属性设为开启"),
                m_AutoLoop);

            EditorGUILayout.EndVertical();
        }

        // ── 预制体列表预览 ────────────────────────────────────
        private void DrawPrefabList()
        {
            if (m_NeedRefresh)
                RefreshPrefabList();

            EditorGUILayout.BeginVertical(m_BoxStyle);
            GUILayout.Label($"📦 预制体列表  ({m_FoundPrefabs.Count} 个)", EditorStyles.boldLabel);
            GUILayout.Space(4);

            if (m_FoundPrefabs.Count == 0)
            {
                EditorGUILayout.HelpBox("未找到预制体，请检查路径是否正确。", MessageType.Info);
            }
            else
            {
                // 最多显示 20 条以节省空间
                int showCount = Mathf.Min(m_FoundPrefabs.Count, 20);
                for (int i = 0; i < showCount; i++)
                {
                    EditorGUILayout.LabelField($"  {i + 1}. {Path.GetFileNameWithoutExtension(m_FoundPrefabs[i])}",
                        EditorStyles.miniLabel);
                }
                if (m_FoundPrefabs.Count > 20)
                    EditorGUILayout.LabelField($"  ... 还有 {m_FoundPrefabs.Count - 20} 个", EditorStyles.miniLabel);
            }

            EditorGUILayout.EndVertical();
        }

        // ── 操作按钮 ──────────────────────────────────────────
        private void DrawActions()
        {
            EditorGUILayout.BeginVertical(m_BoxStyle);
            GUILayout.Label("▶ 操作", EditorStyles.boldLabel);
            GUILayout.Space(4);

            GUI.enabled = m_FoundPrefabs.Count > 0;
            Color oldColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.4f, 0.85f, 0.5f);
            if (GUILayout.Button("🚀  生成预览到场景", GUILayout.Height(36)))
                GeneratePreview();
            GUI.backgroundColor = oldColor;
            GUI.enabled = true;

            GUILayout.Space(4);

            GUI.backgroundColor = new Color(0.9f, 0.45f, 0.4f);
            if (GUILayout.Button("🗑  清除已生成预览", GUILayout.Height(30)))
                ClearPreview();
            GUI.backgroundColor = oldColor;

            EditorGUILayout.EndVertical();
        }

        // ─────────────────────────────────────────────────────
        //  核心逻辑
        // ─────────────────────────────────────────────────────

        /// <summary>扫描目录，刷新预制体路径列表</summary>
        private void RefreshPrefabList()
        {
            m_NeedRefresh = false;
            m_FoundPrefabs.Clear();

            if (string.IsNullOrEmpty(m_FolderPath) || !Directory.Exists(m_FolderPath))
            {
                Repaint();
                return;
            }

            var searchOption = m_Recursive
                ? SearchOption.AllDirectories
                : SearchOption.TopDirectoryOnly;

            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { m_FolderPath });
            m_FoundPrefabs = guids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => p.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase))
                .OrderBy(p => Path.GetFileNameWithoutExtension(p))
                .ToList();

            // 若不含子目录则手动过滤
            if (!m_Recursive)
            {
                string normalizedFolder = m_FolderPath.Replace('\\', '/').TrimEnd('/');
                m_FoundPrefabs = m_FoundPrefabs
                    .Where(p =>
                    {
                        string dir = Path.GetDirectoryName(p)?.Replace('\\', '/');
                        return dir == normalizedFolder;
                    })
                    .ToList();
            }

            SavePrefs();
            Repaint();
        }

        /// <summary>批量实例化预制体并按 Grid 排列到场景</summary>
        private void GeneratePreview()
        {
            if (m_FoundPrefabs.Count == 0) return;

            // 先清除旧的预览组
            ClearPreview(registerUndo: false);

            // 创建父节点
            var groupGo = new GameObject(GroupName);
            Undo.RegisterCreatedObjectUndo(groupGo, "Effect Preview - Generate");

            int col = Mathf.Max(1, m_Columns);

            for (int i = 0; i < m_FoundPrefabs.Count; i++)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(m_FoundPrefabs[i]);
                if (prefab == null) continue;

                int row = i / col;
                int c   = i % col;

                // 计算 Grid 坐标（Y 固定为 0）
                var pos = new Vector3(c * m_SpacingX, 0f, -row * m_SpacingZ);

                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                if (instance == null) continue;

                instance.name             = prefab.name;
                instance.transform.parent = groupGo.transform;

                // 将位置设定：保持预制体本身 Y 偏移，底部贴 Y=0
                instance.transform.localPosition = pos;

                // 自动开启粒子 Loop
                if (m_AutoLoop)
                {
                    foreach (var ps in instance.GetComponentsInChildren<ParticleSystem>(true))
                    {
                        var main = ps.main;
                        main.loop = true;
                    }
                }

                Undo.RegisterCreatedObjectUndo(instance, "Effect Preview - Spawn");
            }

            // 在 Hierarchy 中选中并展开组对象
            Selection.activeGameObject = groupGo;
            EditorGUIUtility.PingObject(groupGo);

            Debug.Log($"[EffectPreviewTool] 已生成 {m_FoundPrefabs.Count} 个预制体预览（Group: {GroupName}）");
        }

        /// <summary>清除场景中所有由本工具生成的预览对象</summary>
        private void ClearPreview(bool registerUndo = true)
        {
            var toDelete = new List<GameObject>();

            // 查找所有顶层 GroupName 对象
            var roots = UnityEngine.SceneManagement.SceneManager
                .GetActiveScene()
                .GetRootGameObjects();

            foreach (var go in roots)
            {
                if (go.name == GroupName)
                    toDelete.Add(go);
            }

            if (toDelete.Count == 0) return;

            foreach (var go in toDelete)
            {
                if (registerUndo)
                    Undo.DestroyObjectImmediate(go);
                else
                    DestroyImmediate(go);
            }

            Debug.Log($"[EffectPreviewTool] 已清除 {toDelete.Count} 个预览组。");
        }
    }
}
