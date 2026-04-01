using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LcLTools
{
    /// <summary>
    /// 将六张 Sided 面贴图（命名格式如 108431_X+.png）转换输出为：
    ///   模式1：Cubemap 十字展开图（TGA，横向 4×3 布局）
    ///   模式2：等距柱状全景图（EXR / HDR，2:1 比例）
    /// </summary>
    public class SidedToCubemapWindow : EditorWindow
    {
        // ──────────────── 输出模式 ────────────────
        private enum OutputMode
        {
            [InspectorName("LDR（TGA 十字展开）")]
            CubemapCross_TGA,
            [InspectorName("HDR（EXR 全景图）")]
            Panorama_EXR,
        }

        // ──────────────── 面贴图映射 ────────────────
        private static readonly string[] FaceKeys = { "X+", "X-", "Y+", "Y-", "Z+", "Z-" };

        private static readonly Dictionary<string, CubemapFace> FaceSuffixMap =
            new Dictionary<string, CubemapFace>
            {
                { "X+", CubemapFace.PositiveX },
                { "X-", CubemapFace.NegativeX },
                { "Y+", CubemapFace.PositiveY },
                { "Y-", CubemapFace.NegativeY },
                { "Z+", CubemapFace.PositiveZ },
                { "Z-", CubemapFace.NegativeZ },
            };

        private static readonly Regex FaceRegex =
            new Regex(@"^(.+?)_(X\+|X-|Y\+|Y-|Z\+|Z-)$", RegexOptions.IgnoreCase);

        // ──────────────── 十字布局定义 ────────────────
        // 视觉布局（从上到下）：
        //   [  ][+Y][  ][  ]
        //   [-X][+Z][+X][-Z]
        //   [  ][-Y][  ][  ]
        // Unity 纹理坐标（0=左下角），故底行 row=0，中行 row=1，顶行 row=2
        // faceIdx 对应 FaceKeys 下标：0=X+,1=X-,2=Y+,3=Y-,4=Z+,5=Z-
        private static readonly int[] CrossFaceIdx = { 0, 1, 2, 3, 4, 5 };
        private static readonly int[] CrossCol      = { 2, 0, 1, 1, 1, 3 }; // X+→col2, X-→col0, Y+→col1, Y-→col1, Z+→col1, Z-→col3
        private static readonly int[] CrossRow      = { 1, 1, 2, 0, 1, 1 }; // X+→mid,  X-→mid,  Y+→top,  Y-→bot,  Z+→mid,  Z-→mid

        // ──────────────── 窗口状态 ────────────────
        private Texture2D[] m_FaceSlots    = new Texture2D[6];
        private bool        m_IsDraggingOver = false;
        private string      m_OutputName   = "Cubemap";
        private Vector2     m_Scroll;
        private string      m_StatusMsg    = "";
        private MessageType m_StatusType   = MessageType.None;
        private OutputMode  m_OutputMode   = OutputMode.Panorama_EXR;
        private bool        m_FlipY        = false;

        // ──────────────── 菜单入口 ────────────────
        [MenuItem("LcLTools/纹理工具/Sided 贴图转 Cubemap")]
        public static void ShowWindow()
        {
            var win = GetWindow<SidedToCubemapWindow>("Sided → Cubemap");
            win.minSize = new Vector2(340, 440);
            win.Show();
        }

        [MenuItem("Assets/LcL Image Tools/Sided 贴图转 Cubemap", false, 20)]
        public static void OpenFromSelection()
        {
            var tex = Selection.activeObject as Texture2D;
            if (tex == null) return;
            var win = GetWindow<SidedToCubemapWindow>("Sided → Cubemap");
            win.minSize = new Vector2(340, 440);
            win.AutoFillFromTexture(tex);
            win.Show();
        }

        [MenuItem("Assets/LcL Image Tools/Sided 贴图转 Cubemap", true)]
        public static bool OpenFromSelectionValidate() => Selection.activeObject is Texture2D;

        // ──────────────── 自动填充 ────────────────
        public void AutoFillFromTexture(Texture2D tex)
        {
            string assetPath     = AssetDatabase.GetAssetPath(tex);
            string nameWithoutExt = Path.GetFileNameWithoutExtension(assetPath);
            string directory     = Path.GetDirectoryName(assetPath);

            var match = FaceRegex.Match(nameWithoutExt);
            if (!match.Success)
            {
                SetStatus($"无法识别面信息，文件名应为 \"前缀_X+\" 格式：{nameWithoutExt}", MessageType.Warning);
                return;
            }

            string prefix = match.Groups[1].Value;
            m_OutputName  = prefix + "_Cubemap";
            FillSlotsFromDirectory(prefix, directory);
            Repaint();
        }

        private void FillSlotsFromDirectory(string prefix, string directory)
        {
            for (int i = 0; i < FaceKeys.Length; i++)
            {
                string key    = FaceKeys[i];
                string[] guids = AssetDatabase.FindAssets($"t:Texture2D {prefix}_{key}", new[] { directory });
                m_FaceSlots[i] = null;
                foreach (var guid in guids)
                {
                    string p = AssetDatabase.GUIDToAssetPath(guid);
                    if (string.Equals(Path.GetFileNameWithoutExtension(p), $"{prefix}_{key}",
                            System.StringComparison.OrdinalIgnoreCase))
                    {
                        m_FaceSlots[i] = AssetDatabase.LoadAssetAtPath<Texture2D>(p);
                        break;
                    }
                }
            }
            SetStatus($"已自动识别前缀 \"{prefix}\"，请检查六个面槽位。", MessageType.Info);
        }

        // ──────────────── GUI ────────────────
        private void OnGUI()
        {
            m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);

            DrawHeader();
            EditorGUILayout.Space(6);
            DrawDropArea();
            EditorGUILayout.Space(6);
            DrawFaceSlots();
            EditorGUILayout.Space(6);
            DrawOutputSettings();
            EditorGUILayout.Space(6);
            DrawGenerateButton();

            if (!string.IsNullOrEmpty(m_StatusMsg))
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.HelpBox(m_StatusMsg, m_StatusType);
            }

            EditorGUILayout.Space(6);
            EditorGUILayout.EndScrollView();

            HandleDragAndDrop();
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("Sided → Cubemap / 全景图",
                new GUIStyle(EditorStyles.boldLabel) { fontSize = 13, alignment = TextAnchor.MiddleCenter },
                GUILayout.Height(24));
        }

        private void DrawDropArea()
        {
            bool   dragging   = m_IsDraggingOver;
            Color  border     = dragging ? new Color(0.2f, 0.6f, 1f)            : new Color(0.4f, 0.4f, 0.4f);
            Color  bg         = dragging ? new Color(0.2f, 0.4f, 0.6f, 0.3f)   : new Color(0.3f, 0.3f, 0.3f, 0.2f);
            string label      = dragging ? "松开鼠标以填充面贴图"
                                         : "将六张面贴图拖拽到此处\n（自动识别 X+/X-/Y+/Y-/Z+/Z- 命名）";

            var rect = GUILayoutUtility.GetRect(0, 60, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, bg);
            DrawBorder(rect, border, 2);
            GUI.Label(rect, label, new GUIStyle(EditorStyles.label)
            {
                alignment  = TextAnchor.MiddleCenter,
                fontStyle  = dragging ? FontStyle.Bold : FontStyle.Normal,
                fontSize   = 12,
                wordWrap   = true
            });
        }

        private static void DrawBorder(Rect r, Color c, float t)
        {
            EditorGUI.DrawRect(new Rect(r.x,        r.y,         r.width, t),        c);
            EditorGUI.DrawRect(new Rect(r.x,        r.yMax - t,  r.width, t),        c);
            EditorGUI.DrawRect(new Rect(r.x,        r.y,         t,       r.height), c);
            EditorGUI.DrawRect(new Rect(r.xMax - t, r.y,         t,       r.height), c);
        }

        private const float SlotSize = 56f;

        private void DrawFaceSlots()
        {
            EditorGUILayout.LabelField("面贴图槽位", EditorStyles.boldLabel);

            var labelStyle = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleRight };

            // 每两个面为一行：X+/X-，Y+/Y-，Z+/Z-
            for (int i = 0; i < FaceKeys.Length; i += 2)
            {
                EditorGUILayout.BeginHorizontal();

                // 左面
                GUILayout.Label(FaceKeys[i], labelStyle, GUILayout.Width(26), GUILayout.Height(SlotSize));
                GUILayout.Space(2);
                m_FaceSlots[i] = (Texture2D)EditorGUILayout.ObjectField(
                    m_FaceSlots[i], typeof(Texture2D), false,
                    GUILayout.Width(SlotSize), GUILayout.Height(SlotSize));

                GUILayout.Space(12);

                // 右面
                GUILayout.Label(FaceKeys[i + 1], labelStyle, GUILayout.Width(26), GUILayout.Height(SlotSize));
                GUILayout.Space(2);
                m_FaceSlots[i + 1] = (Texture2D)EditorGUILayout.ObjectField(
                    m_FaceSlots[i + 1], typeof(Texture2D), false,
                    GUILayout.Width(SlotSize), GUILayout.Height(SlotSize));

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(4);
            }
        }

        private void DrawOutputSettings()
        {
            EditorGUILayout.LabelField("输出设置", EditorStyles.boldLabel);

            m_OutputMode = (OutputMode)EditorGUILayout.EnumPopup("输出模式", m_OutputMode);
            m_OutputName = EditorGUILayout.TextField("文件名", m_OutputName);
            m_FlipY = EditorGUILayout.Toggle(
                new GUIContent("翻转 Y 轴", "Y+（天空）与 Y-（地面）互换，用于修正天空盒方向"),
                m_FlipY);

            if (m_OutputMode == OutputMode.Panorama_EXR)
            {
                int faceSize   = m_FaceSlots[0] != null ? m_FaceSlots[0].width : 0;
                int panoW      = faceSize * 4;
                int panoH      = faceSize * 2;
                string sizeStr = faceSize > 0
                    ? $"输出尺寸：{panoW} × {panoH} px（面尺寸 {faceSize} × 4/2）"
                    : "输出尺寸：待填入贴图后自动计算";
                EditorGUILayout.LabelField(sizeStr, EditorStyles.miniLabel);
            }

            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("清空槽位", GUILayout.Height(22)))
            {
                for (int i = 0; i < m_FaceSlots.Length; i++) m_FaceSlots[i] = null;
                SetStatus("", MessageType.None);
            }
            if (GUILayout.Button("从选中贴图自动识别", GUILayout.Height(22)))
            {
                var tex = Selection.activeObject as Texture2D;
                if (tex != null) AutoFillFromTexture(tex);
                else SetStatus("请先在 Project 视图中选中一张面贴图。", MessageType.Warning);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawGenerateButton()
        {
            bool allFilled = true;
            foreach (var t in m_FaceSlots)
                if (t == null) { allFilled = false; break; }

            string btnText = m_OutputMode == OutputMode.CubemapCross_TGA
                ? "生成 LDR Cubemap（TGA）"
                : "生成 HDR 全景图（EXR）";

            if (!allFilled)
                EditorGUILayout.HelpBox("请填充全部六个面贴图槽位后再生成。", MessageType.Warning);

            GUI.enabled = allFilled;
            if (GUILayout.Button(btnText, GUILayout.Height(34)))
            {
                if (m_OutputMode == OutputMode.CubemapCross_TGA)
                    GenerateCubemapCrossTGA();
                else
                    GeneratePanoramaEXR();
            }
            GUI.enabled = true;
        }

        // ──────────────── 拖拽处理 ────────────────
        private void HandleDragAndDrop()
        {
            var ev = Event.current;
            if (ev == null) return;

            switch (ev.type)
            {
                case EventType.DragUpdated:
                    if (ContainsTextures(DragAndDrop.objectReferences))
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        m_IsDraggingOver = true;
                        ev.Use();
                        Repaint();
                    }
                    break;

                case EventType.DragPerform:
                    if (ContainsTextures(DragAndDrop.objectReferences))
                    {
                        DragAndDrop.AcceptDrag();
                        ProcessDroppedTextures(DragAndDrop.objectReferences);
                        m_IsDraggingOver = false;
                        ev.Use();
                        Repaint();
                    }
                    break;

                case EventType.DragExited:
                    m_IsDraggingOver = false;
                    Repaint();
                    break;
            }
        }

        private static bool ContainsTextures(Object[] objs)
        {
            foreach (var o in objs)
                if (o is Texture2D) return true;
            return false;
        }

        private void ProcessDroppedTextures(Object[] objs)
        {
            var textures = new List<Texture2D>();
            foreach (var o in objs)
                if (o is Texture2D t) textures.Add(t);
            if (textures.Count == 0) return;

            bool anyMatched = false;
            foreach (var tex in textures)
            {
                string nameWithoutExt = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(tex));
                var match = FaceRegex.Match(nameWithoutExt);
                if (match.Success)
                {
                    string faceKey = match.Groups[2].Value;
                    int idx = System.Array.IndexOf(FaceKeys, faceKey);
                    if (idx >= 0)
                    {
                        m_FaceSlots[idx] = tex;
                        anyMatched = true;

                        // 只拖一张 → 自动填充同前缀其余面
                        if (textures.Count == 1)
                        {
                            string prefix = match.Groups[1].Value;
                            string dir    = Path.GetDirectoryName(AssetDatabase.GetAssetPath(tex));
                            m_OutputName  = prefix + "_Cubemap";
                            FillSlotsFromDirectory(prefix, dir);
                            return;
                        }
                    }
                }
            }

            SetStatus(anyMatched
                ? "已按名称自动分配面贴图到槽位。"
                : "拖入的贴图无法识别面信息，请确认命名格式为 \"前缀_X+\"。",
                anyMatched ? MessageType.Info : MessageType.Warning);
        }

        // ──────────────── 生成：Cubemap 十字展开 TGA ────────────────
        //   最终图尺寸：4*s × 3*s（s = 单面分辨率）
        //   布局（视觉，从上到下）：
        //     [  ][+Y][  ][  ]   row=2（top）
        //     [-X][+Z][+X][-Z]   row=1（mid）
        //     [  ][-Y][  ][  ]   row=0（bot）
        private void GenerateCubemapCrossTGA()
        {
            EnsureTexturesReadable();
            int s = m_FaceSlots[0].width;
            if (!CheckSizes(s)) return;

            int w = s * 4, h = s * 3;
            var cross = new Texture2D(w, h, TextureFormat.RGBA32, false);

            // 填充透明底色
            var clear = new Color[w * h];
            cross.SetPixels(clear);

            // 按布局表复制各面像素（Y 轴翻转时 Y+ 和 Y- 互换）
            for (int i = 0; i < FaceKeys.Length; i++)
            {
                int col = CrossCol[i];
                int row = CrossRow[i];
                // FaceKeys[2]=Y+, FaceKeys[3]=Y-
                int srcIdx = m_FlipY && (i == 2 || i == 3) ? (i == 2 ? 3 : 2) : i;
                Color[] pixels = m_FaceSlots[srcIdx].GetPixels();
                cross.SetPixels(col * s, row * s, s, s, pixels);
            }
            cross.Apply();

            // 保存 TGA
            string absPath   = GetAbsSavePath(".tga");
            string assetPath = GetRelativeSavePath(".tga");
            File.WriteAllBytes(absPath, cross.EncodeToTGA());
            DestroyImmediate(cross);

            AssetDatabase.Refresh();
            PingAsset(assetPath);
            SetStatus($"✅ TGA 十字展开图已生成：{assetPath}", MessageType.Info);
            Debug.Log($"[SidedToCubemap] TGA 已生成：{absPath}");
        }

        // ──────────────── 生成：等距柱状全景图 EXR ────────────────
        //   对每个输出像素计算球面方向 → 找对应立方体面 → 双线性采样
        private void GeneratePanoramaEXR()
        {
            SetStatus("正在生成全景图，请稍候...", MessageType.Info);

            EnsureTexturesReadable();
            int s = m_FaceSlots[0].width;
            if (!CheckSizes(s)) return;

            // 缓存各面像素（Y 轴翻转时 Y+ 和 Y- 互换）
            var facePixels = new Color[6][];
            for (int i = 0; i < 6; i++)
            {
                // FaceKeys[2]=Y+, FaceKeys[3]=Y-
                int srcIdx = m_FlipY && (i == 2 || i == 3) ? (i == 2 ? 3 : 2) : i;
                facePixels[i] = m_FaceSlots[srcIdx].GetPixels();
            }

            int outW = s * 4;
            int outH = s * 2;
            var outPixels = new Color[outW * outH];

            // EXR 编码强制要求 RGBAFloat 格式，无论输入贴图是否 HDR 都统一使用
            var panorama = new Texture2D(outW, outH, TextureFormat.RGBAFloat, false);

            try
            {
                for (int y = 0; y < outH; y++)
                {
                    // v: 0（上）→1（下），转为纬度 π/2（上）→ -π/2（下）
                    float lat = ((outH - 1 - y + 0.5f) / outH - 0.5f) * Mathf.PI;
                    float cosLat = Mathf.Cos(lat);
                    float sinLat = Mathf.Sin(lat);

                    for (int x = 0; x < outW; x++)
                    {
                        // u: 0→1，转为经度 -π→+π
                        float lon = ((x + 0.5f) / outW - 0.5f) * 2f * Mathf.PI;

                        // 球面坐标 → 方向向量（Y-up，右手系）
                        float dx = cosLat * Mathf.Sin(lon);
                        float dy = sinLat;
                        float dz = cosLat * Mathf.Cos(lon);

                        outPixels[y * outW + x] = SampleCubemapFaces(dx, dy, dz, facePixels, s);
                    }

                    if (y % 64 == 0) // 进度提示（文件可能很大）
                        EditorUtility.DisplayProgressBar(
                            "生成全景图", $"正在处理行 {y + 1} / {outH}...", (float)y / outH);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            panorama.SetPixels(outPixels);
            panorama.Apply();

            string absPath   = GetAbsSavePath(".exr");
            string assetPath = GetRelativeSavePath(".exr");
            File.WriteAllBytes(absPath, panorama.EncodeToEXR(Texture2D.EXRFlags.OutputAsFloat));
            DestroyImmediate(panorama);

            AssetDatabase.Refresh();
            PingAsset(assetPath);
            SetStatus($"✅ EXR 全景图已生成：{assetPath}", MessageType.Info);
            Debug.Log($"[SidedToCubemap] EXR 已生成：{absPath}");
        }

        // ──────────────── 立方体面采样 ────────────────
        // 标准 OpenGL Cubemap 采样公式（Y-up，右手坐标系）
        private static Color SampleCubemapFaces(float dx, float dy, float dz,
            Color[][] facePixels, int faceSize)
        {
            float ax = Mathf.Abs(dx), ay = Mathf.Abs(dy), az = Mathf.Abs(dz);
            int   faceIdx;
            float u, v;

            if (ax >= ay && ax >= az)
            {
                if (dx > 0) { faceIdx = 0; u = -dz / ax; v =  dy / ax; } // +X
                else        { faceIdx = 1; u =  dz / ax; v =  dy / ax; } // -X
            }
            else if (ay >= ax && ay >= az)
            {
                if (dy > 0) { faceIdx = 2; u =  dx / ay; v = -dz / ay; } // +Y
                else        { faceIdx = 3; u =  dx / ay; v =  dz / ay; } // -Y
            }
            else
            {
                if (dz > 0) { faceIdx = 4; u =  dx / az; v =  dy / az; } // +Z
                else        { faceIdx = 5; u = -dx / az; v =  dy / az; } // -Z
            }

            // [-1,1] → [0,1]
            u = Mathf.Clamp01((u + 1f) * 0.5f);
            v = Mathf.Clamp01((v + 1f) * 0.5f);

            return SampleBilinear(facePixels[faceIdx], faceSize, u, v);
        }

        private static Color SampleBilinear(Color[] pixels, int size, float u, float v)
        {
            float px = u * (size - 1);
            float py = v * (size - 1);
            int x0 = Mathf.FloorToInt(px), x1 = Mathf.Min(x0 + 1, size - 1);
            int y0 = Mathf.FloorToInt(py), y1 = Mathf.Min(y0 + 1, size - 1);
            float tx = px - x0, ty = py - y0;

            Color c00 = pixels[y0 * size + x0];
            Color c10 = pixels[y0 * size + x1];
            Color c01 = pixels[y1 * size + x0];
            Color c11 = pixels[y1 * size + x1];
            return Color.Lerp(Color.Lerp(c00, c10, tx), Color.Lerp(c01, c11, tx), ty);
        }

        // ──────────────── 工具方法 ────────────────
        private bool CheckSizes(int expected)
        {
            for (int i = 1; i < m_FaceSlots.Length; i++)
            {
                if (m_FaceSlots[i].width != expected || m_FaceSlots[i].height != expected)
                {
                    SetStatus($"面贴图尺寸不一致！{FaceKeys[0]}={expected}px，{FaceKeys[i]}={m_FaceSlots[i].width}px。",
                        MessageType.Error);
                    return false;
                }
            }
            return true;
        }

        /// <summary>返回输出文件的 Assets 相对路径（用于 AssetDatabase）。</summary>
        private string GetRelativeSavePath(string ext)
        {
            string firstAssetPath = AssetDatabase.GetAssetPath(m_FaceSlots[0]);
            string relDir = Path.GetDirectoryName(firstAssetPath).Replace("\\", "/");
            return $"{relDir}/{m_OutputName}{ext}";
        }

        /// <summary>返回输出文件的磁盘绝对路径（用于 File.WriteAllBytes）。</summary>
        private string GetAbsSavePath(string ext)
        {
            string relPath = GetRelativeSavePath(ext);
            // relPath 形如 "Assets/Raw/Textures/xxx.tga"
            // Application.dataPath 形如 "F:/Project/Assets"
            return Path.Combine(
                Application.dataPath,
                relPath.Substring("Assets/".Length)
            ).Replace("\\", "/");
        }

        private static void PingAsset(string assetPath)
        {
            var obj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (obj == null) return;
            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);
        }

        private void EnsureTexturesReadable()
        {
            bool needRefresh = false;
            foreach (var tex in m_FaceSlots)
            {
                if (tex == null) continue;
                string path    = AssetDatabase.GetAssetPath(tex);
                var importer   = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null && !importer.isReadable)
                {
                    importer.isReadable = true;
                    importer.SaveAndReimport();
                    needRefresh = true;
                }
            }
            if (needRefresh) AssetDatabase.Refresh();
        }

        private void SetStatus(string msg, MessageType type)
        {
            m_StatusMsg  = msg;
            m_StatusType = type;
        }
    }
}