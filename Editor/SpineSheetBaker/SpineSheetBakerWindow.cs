using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Spine.Unity;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SpineSheetBaker.Editor {
	public sealed class SpineSheetBakerWindow : EditorWindow {
		const string PreferencesKey = "SpineSheetBaker.EditorWindow.Settings.v2";
		const string FfmpegPreferencesKey = "SpineSheetBaker.EditorWindow.FFmpeg";
		SpineSheetBakeRequest request = new SpineSheetBakeRequest();
		readonly List<string> availableAnimations = new List<string>();
		readonly List<string> availableSkins = new List<string>();
		Vector2 mainScroll;
		Vector2 animationScroll;
		Vector2 skinScroll;
		bool showSampling = true;
		bool showFraming = true;
		bool showQuality = true;
		bool showAtlas = true;
		bool showOutput = true;
		bool showPreview = true;
		Texture2D previewTexture;
		float previewTime;
		bool previewPlaying;
		double lastPreviewUpdate;
		SpineSheetBakerPreset preset;

		[MenuItem("LcLTools/Spine/序列帧图集烘焙器")]
		public static SpineSheetBakerWindow Open () {
			SpineSheetBakerWindow window = GetWindow<SpineSheetBakerWindow>();
			window.titleContent = new GUIContent("Spine 图集烘焙");
			window.minSize = new Vector2(600f, 680f);
			window.Show();
			return window;
		}

		[MenuItem("Assets/LcLTools/用序列帧图集烘焙", true)]
		static bool ValidateOpenFromAsset () => Selection.activeObject is SkeletonDataAsset;

		[MenuItem("Assets/LcLTools/用序列帧图集烘焙", false, 2000)]
		static void OpenFromAsset () {
			SpineSheetBakerWindow window = Open();
			window.SetSource(Selection.activeObject as SkeletonDataAsset);
		}

		void OnEnable () {
			string json = EditorPrefs.GetString(PreferencesKey, string.Empty);
			if (!string.IsNullOrEmpty(json)) {
				try {
					SpineSheetBakeRequest saved = JsonUtility.FromJson<SpineSheetBakeRequest>(json);
					if (saved != null) request = saved;
				} catch (Exception exception) {
					Debug.LogWarning("无法恢复 Spine 图集烘焙器设置：" + exception.Message);
				}
			}
			if (request.AnimationNames == null) request.AnimationNames = new List<string>();
			if (request.SkinNames == null) request.SkinNames = new List<string>();
			request.FfmpegExecutable = EditorPrefs.GetString(FfmpegPreferencesKey, "ffmpeg");
			if (string.IsNullOrWhiteSpace(request.FfmpegExecutable)) request.FfmpegExecutable = "ffmpeg";
			if (request.Source != null) RefreshSourceLists(false);
			EditorApplication.update += OnEditorUpdate;
		}

		void OnDisable () {
			EditorApplication.update -= OnEditorUpdate;
			EditorPrefs.SetString(PreferencesKey, JsonUtility.ToJson(request));
			EditorPrefs.SetString(FfmpegPreferencesKey,
				string.IsNullOrWhiteSpace(request.FfmpegExecutable) ? "ffmpeg" : request.FfmpegExecutable);
			DestroyPreview();
		}

		void OnGUI () {
			mainScroll = EditorGUILayout.BeginScrollView(mainScroll, GUILayout.ExpandHeight(true));
			DrawSource();
			DrawSelectionLists();
			DrawSampling();
			DrawFraming();
			DrawQuality();
			DrawAtlas();
			DrawOutput();
			DrawPreview();
			DrawEstimate();
			EditorGUILayout.EndScrollView();

			using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar, GUILayout.Height(34f))) {
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("重置", GUILayout.Width(90f), GUILayout.Height(24f))) ResetSettings();
				GUI.enabled = CanBake();
				if (GUILayout.Button("开始烘焙", GUILayout.Width(140f), GUILayout.Height(24f))) Bake();
				GUI.enabled = true;
			}
		}

		void DrawSource () {
			SectionHeader("源骨架");
			EditorGUI.BeginChangeCheck();
			SkeletonDataAsset source = (SkeletonDataAsset)EditorGUILayout.ObjectField("骨骼数据", request.Source,
				typeof(SkeletonDataAsset), false);
			if (EditorGUI.EndChangeCheck()) SetSource(source);
		}

		void DrawSelectionLists () {
			if (request.Source == null) {
				EditorGUILayout.HelpBox("拖入或选择一个 SkeletonDataAsset 后即可读取动画和 Skin。", MessageType.Info);
				return;
			}

			using (new EditorGUILayout.HorizontalScope()) {
				using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.MinWidth(280f))) {
					DrawListToolbar("动画", availableAnimations, request.AnimationNames);
					animationScroll = EditorGUILayout.BeginScrollView(animationScroll, GUILayout.Height(120f));
					DrawToggleList(availableAnimations, request.AnimationNames);
					EditorGUILayout.EndScrollView();
				}
				using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.MinWidth(250f))) {
					DrawListToolbar("Skin", availableSkins, request.SkinNames);
					skinScroll = EditorGUILayout.BeginScrollView(skinScroll, GUILayout.Height(120f));
					DrawToggleList(availableSkins, request.SkinNames);
					EditorGUILayout.EndScrollView();
				}
			}
		}

		void DrawSampling () {
			showSampling = EditorGUILayout.BeginFoldoutHeaderGroup(showSampling, "采样");
			if (showSampling) {
				using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
					request.SamplingMode = (SpineSheetSamplingMode)EditorGUILayout.EnumPopup("采样模式", request.SamplingMode);
					if (request.SamplingMode == SpineSheetSamplingMode.FramesPerSecond)
						request.FramesPerSecond = EditorGUILayout.IntSlider("FPS", request.FramesPerSecond, 1, 120);
					else
						request.ExactFrameCount = EditorGUILayout.IntField("帧数", Mathf.Max(1, request.ExactFrameCount));
					request.Loop = EditorGUILayout.Toggle("循环", request.Loop);
					request.UseCustomRange = EditorGUILayout.Toggle("自定义时间段", request.UseCustomRange);
					if (request.UseCustomRange) {
						using (new EditorGUILayout.HorizontalScope()) {
							request.RangeStart = EditorGUILayout.FloatField("开始", Mathf.Max(0f, request.RangeStart));
							request.RangeEnd = EditorGUILayout.FloatField("结束", Mathf.Max(0f, request.RangeEnd));
						}
					}
				}
			}
			EditorGUILayout.EndFoldoutHeaderGroup();
		}

		void DrawFraming () {
			showFraming = EditorGUILayout.BeginFoldoutHeaderGroup(showFraming, "取景与尺寸");
			if (showFraming) {
				using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
					request.BoundsMode = (SpineSheetBoundsMode)EditorGUILayout.EnumPopup("取景模式", request.BoundsMode);
					if (request.BoundsMode == SpineSheetBoundsMode.Custom) {
						request.CustomBounds.position = EditorGUILayout.Vector2Field("取景框 Min", request.CustomBounds.position);
						request.CustomBounds.size = EditorGUILayout.Vector2Field("取景框 Size", request.CustomBounds.size);
					}
					request.BoundsPadding = EditorGUILayout.FloatField("世界单位留白", Mathf.Max(0f, request.BoundsPadding));
					request.SizingMode = (SpineSheetSizingMode)EditorGUILayout.EnumPopup("尺寸模式", request.SizingMode);
					if (request.SizingMode == SpineSheetSizingMode.PixelsPerUnit)
						request.PixelsPerUnit = EditorGUILayout.IntField("Pixels Per Unit", Mathf.Max(1, request.PixelsPerUnit));
					else {
						using (new EditorGUILayout.HorizontalScope()) {
							request.FrameWidth = EditorGUILayout.IntField("宽", Mathf.Max(1, request.FrameWidth));
							request.FrameHeight = EditorGUILayout.IntField("高", Mathf.Max(1, request.FrameHeight));
						}
					}
					using (new EditorGUILayout.HorizontalScope()) {
						request.FlipX = EditorGUILayout.Toggle("水平翻转", request.FlipX);
						request.FlipY = EditorGUILayout.Toggle("垂直翻转", request.FlipY);
					}
				}
			}
			EditorGUILayout.EndFoldoutHeaderGroup();
		}

		void DrawQuality () {
			showQuality = EditorGUILayout.BeginFoldoutHeaderGroup(showQuality, "渲染质量");
			if (showQuality) {
				using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
					request.Msaa = EditorGUILayout.IntPopup("MSAA", request.Msaa,
						new[] { "关闭", "2x", "4x", "8x" }, new[] { 1, 2, 4, 8 });
					request.Supersampling = EditorGUILayout.IntPopup("超采样", request.Supersampling,
						new[] { "1x", "2x", "4x" }, new[] { 1, 2, 4 });
					request.FilterMode = (FilterMode)EditorGUILayout.EnumPopup("过滤", request.FilterMode);
					request.AlphaMode = (SpineSheetAlphaMode)EditorGUILayout.EnumPopup("Alpha 输出", request.AlphaMode);
				}
			}
			EditorGUILayout.EndFoldoutHeaderGroup();
		}

		void DrawAtlas () {
			showAtlas = EditorGUILayout.BeginFoldoutHeaderGroup(showAtlas, "图集");
			if (showAtlas) {
				using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
					request.MaxAtlasSize = EditorGUILayout.IntPopup("最大尺寸", request.MaxAtlasSize,
						new[] { "1024", "2048", "4096", "8192" }, new[] { 1024, 2048, 4096, 8192 });
					using (new EditorGUILayout.HorizontalScope()) {
						request.Padding = EditorGUILayout.IntField("Padding", Mathf.Max(0, request.Padding));
						request.EdgeExtrude = EditorGUILayout.IntField("边缘扩展", Mathf.Clamp(request.EdgeExtrude, 0, request.Padding / 2));
					}
				request.PowerOfTwoAtlas = EditorGUILayout.Toggle("2 的幂尺寸", request.PowerOfTwoAtlas);
				request.TopDownLayout = EditorGUILayout.Toggle("帧序从上到下", request.TopDownLayout);
				request.GenerateMipMaps = EditorGUILayout.Toggle("Mipmap", request.GenerateMipMaps);
					request.TextureCompression = (TextureImporterCompression)EditorGUILayout.EnumPopup("纹理压缩", request.TextureCompression);
				}
			}
			EditorGUILayout.EndFoldoutHeaderGroup();
		}

		void DrawOutput () {
			showOutput = EditorGUILayout.BeginFoldoutHeaderGroup(showOutput, "输出");
			if (showOutput) {
				using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
					using (new EditorGUILayout.HorizontalScope()) {
						request.OutputFolder = EditorGUILayout.TextField("Assets 目录", request.OutputFolder);
						if (GUILayout.Button("选择", GUILayout.Width(58f))) ChooseOutputFolder();
					}
					request.GenerateAnimationClips = EditorGUILayout.Toggle("生成 UGUI Clip", request.GenerateAnimationClips);
					GUI.enabled = request.GenerateAnimationClips;
					request.GenerateAnimatorController = EditorGUILayout.Toggle("生成 AnimatorController", request.GenerateAnimatorController);
					GUI.enabled = true;
					request.GenerateVideo = EditorGUILayout.Toggle("生成视频（FFmpeg）", request.GenerateVideo);
					if (request.GenerateVideo) {
						using (new EditorGUI.IndentLevelScope()) {
						request.VideoFormat = (SpineSheetVideoFormat)EditorGUILayout.EnumPopup("视频格式", request.VideoFormat);
						request.VideoQuality = EditorGUILayout.IntSlider("质量 CRF（低值更清晰）", request.VideoQuality,
							request.VideoFormat == SpineSheetVideoFormat.Mp4H264 ? 1 : 0, 51);
						request.VideoSizingMode = (SpineSheetVideoSizingMode)EditorGUILayout.EnumPopup("视频分辨率", request.VideoSizingMode);
						if (request.VideoSizingMode == SpineSheetVideoSizingMode.Custom)
							request.VideoScale = EditorGUILayout.Slider("缩放比例", request.VideoScale, 0.1f, 4f);
						if (request.VideoFormat == SpineSheetVideoFormat.Mp4H264)
							request.VideoBackgroundColor = EditorGUILayout.ColorField("透明区背景", request.VideoBackgroundColor);
						request.FfmpegExecutable = EditorGUILayout.TextField("FFmpeg", request.FfmpegExecutable);
							EditorGUILayout.HelpBox(request.VideoFormat == SpineSheetVideoFormat.Mp4H264
								? "MP4/H.264 不支持透明通道，透明区域会合成到所选背景色。"
								: "WebM/VP9 保留透明通道，但 Unity 2021.3 Windows Editor 不支持导入 VP9；文件仍可供支持 VP9 Alpha 的外部播放器或目标平台使用。",
								request.VideoFormat == SpineSheetVideoFormat.Mp4H264 ? MessageType.None : MessageType.Warning);
						}
					}
					using (new EditorGUILayout.HorizontalScope()) {
						preset = (SpineSheetBakerPreset)EditorGUILayout.ObjectField("预设", preset, typeof(SpineSheetBakerPreset), false);
						GUI.enabled = preset != null;
						if (GUILayout.Button("载入", GUILayout.Width(54f))) ApplyPreset(preset);
						GUI.enabled = true;
						if (GUILayout.Button("另存", GUILayout.Width(54f))) SavePreset();
					}
				}
			}
			EditorGUILayout.EndFoldoutHeaderGroup();
		}

		void DrawPreview () {
			showPreview = EditorGUILayout.BeginFoldoutHeaderGroup(showPreview, "实时预览");
			if (showPreview) {
				using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
					string animationName = request.AnimationNames.FirstOrDefault();
					string skinName = request.SkinNames.FirstOrDefault();
					float duration = GetAnimationDuration(animationName);
					using (new EditorGUILayout.HorizontalScope()) {
						GUI.enabled = request.Source != null && duration > 0f && !string.IsNullOrEmpty(skinName);
						if (GUILayout.Button(previewPlaying ? "暂停" : "播放", GUILayout.Width(64f))) {
							previewPlaying = !previewPlaying;
							lastPreviewUpdate = EditorApplication.timeSinceStartup;
						}
						float newTime = EditorGUILayout.Slider(previewTime, 0f, Mathf.Max(0.0001f, duration));
						if (!Mathf.Approximately(newTime, previewTime)) {
							previewTime = newTime;
							RefreshPreview();
						}
						if (GUILayout.Button("刷新", GUILayout.Width(54f))) RefreshPreview();
						GUI.enabled = true;
					}
					Rect rect = GUILayoutUtility.GetAspectRect(1.6f, GUILayout.MaxHeight(320f));
					if (previewTexture != null) EditorGUI.DrawPreviewTexture(rect, previewTexture, null, ScaleMode.ScaleToFit);
					else EditorGUI.DrawRect(rect, new Color(0.12f, 0.12f, 0.12f, 1f));
				}
			}
			EditorGUILayout.EndFoldoutHeaderGroup();
		}

		void DrawEstimate () {
			if (request.Source == null || request.AnimationNames.Count == 0 || request.SkinNames.Count == 0) return;
			int frames = EstimateFrameCount();
			Vector2Int frameSize = EstimateFrameSize();
			int cellWidth = frameSize.x + request.Padding;
			int cellHeight = frameSize.y + request.Padding;
			int columns = Mathf.Max(1, (request.MaxAtlasSize - request.Padding) / Mathf.Max(1, cellWidth));
			int rows = Mathf.Max(1, (request.MaxAtlasSize - request.Padding) / Mathf.Max(1, cellHeight));
			int perPage = Mathf.Max(1, columns * rows);
			int pages = Mathf.CeilToInt(frames / (float)perPage);
			long rawBytes = (long)frames * frameSize.x * frameSize.y * 4L;
			long peakRenderBytes = (long)frameSize.x * frameSize.y * request.Supersampling * request.Supersampling *
				4L * Mathf.Max(1, request.Msaa);
			string sizeNote = request.SizingMode == SpineSheetSizingMode.FixedPixels || previewTexture != null ||
				request.BoundsMode == SpineSheetBoundsMode.Custom ? string.Empty : "（尚未预览，帧尺寸为参考值）";
			string videoEstimate = request.GenerateVideo
				? string.Format("，并生成 {0} 个{1}视频", request.AnimationNames.Count * request.SkinNames.Count,
					request.VideoFormat == SpineSheetVideoFormat.Mp4H264 ? " MP4 " : " WebM ")
				: string.Empty;
			string estimate = string.Format("预计：{0} 个动画/Skin 组合，约 {1} 帧、{2} 页{8}；参考帧尺寸 {3}x{4}{5}。\n" +
				"序列原始像素约 {6:0.0} MB，单帧渲染峰值显存约 {7:0.0} MB。",
				request.AnimationNames.Count * request.SkinNames.Count, frames, pages, frameSize.x, frameSize.y, sizeNote,
				rawBytes / (1024f * 1024f), peakRenderBytes / (1024f * 1024f), videoEstimate);
			EditorGUILayout.HelpBox(estimate, frames > 5000 ? MessageType.Warning : MessageType.Info);
		}

		Vector2Int EstimateFrameSize () {
			if (request.SizingMode == SpineSheetSizingMode.FixedPixels)
				return new Vector2Int(Mathf.Max(1, request.FrameWidth), Mathf.Max(1, request.FrameHeight));
			if (previewTexture != null)
				return new Vector2Int(previewTexture.width, previewTexture.height);
			if (request.BoundsMode == SpineSheetBoundsMode.Custom) {
				int width = Mathf.CeilToInt((request.CustomBounds.width + request.BoundsPadding * 2f) * request.PixelsPerUnit);
				int height = Mathf.CeilToInt((request.CustomBounds.height + request.BoundsPadding * 2f) * request.PixelsPerUnit);
				return new Vector2Int(Mathf.Max(1, width), Mathf.Max(1, height));
			}
			return new Vector2Int(Mathf.Max(1, request.FrameWidth), Mathf.Max(1, request.FrameHeight));
		}

		void SetSource (SkeletonDataAsset source) {
			request.Source = source;
			RefreshSourceLists(true);
			DestroyPreview();
			previewTime = 0f;
			Repaint();
		}

		void RefreshSourceLists (bool selectDefaults) {
			availableAnimations.Clear();
			availableSkins.Clear();
			if (request.Source == null) return;
			Spine.SkeletonData data = request.Source.GetSkeletonData(true);
			if (data == null) return;
			for (int i = 0; i < data.Animations.Count; i++) availableAnimations.Add(data.Animations.Items[i].Name);
			for (int i = 0; i < data.Skins.Count; i++) availableSkins.Add(data.Skins.Items[i].Name);
			if (data.DefaultSkin != null && !availableSkins.Contains(data.DefaultSkin.Name)) availableSkins.Insert(0, data.DefaultSkin.Name);
			request.AnimationNames.RemoveAll(name => !availableAnimations.Contains(name));
			request.SkinNames.RemoveAll(name => !availableSkins.Contains(name));
			if (selectDefaults || request.AnimationNames.Count == 0) {
				request.AnimationNames.Clear();
				if (availableAnimations.Count > 0) request.AnimationNames.Add(availableAnimations[0]);
			}
			if (selectDefaults || request.SkinNames.Count == 0) {
				request.SkinNames.Clear();
				if (availableSkins.Contains("default")) request.SkinNames.Add("default");
				else if (availableSkins.Count > 0) request.SkinNames.Add(availableSkins[0]);
			}
		}

		static void DrawListToolbar (string title, List<string> available, List<string> selected) {
			using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar)) {
				GUILayout.Label(string.Format("{0} ({1}/{2})", title, selected.Count, available.Count));
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("全选", EditorStyles.toolbarButton, GUILayout.Width(42f))) {
					selected.Clear();
					selected.AddRange(available);
				}
				if (GUILayout.Button("反选", EditorStyles.toolbarButton, GUILayout.Width(42f))) {
					HashSet<string> previous = new HashSet<string>(selected, StringComparer.Ordinal);
					selected.Clear();
					selected.AddRange(available.Where(name => !previous.Contains(name)));
				}
			}
		}

		static void DrawToggleList (IEnumerable<string> available, List<string> selected) {
			foreach (string name in available) {
				bool wasSelected = selected.Contains(name);
				bool isSelected = EditorGUILayout.ToggleLeft(name, wasSelected);
				if (isSelected == wasSelected) continue;
				if (isSelected) selected.Add(name);
				else selected.Remove(name);
			}
		}

		void Bake () {
			SpineSheetBakeResult result;
			try {
				result = SpineSheetBakerApi.Bake(request.Clone(), progress => {
					bool cancelled = EditorUtility.DisplayCancelableProgressBar("Spine 序列帧烘焙",
						progress.Stage + "：" + progress.Item, progress.Normalized);
					return !cancelled;
				});
			} finally {
				EditorUtility.ClearProgressBar();
			}

			if (result.Status == SpineSheetBakeStatus.Succeeded) {
				Object manifest = result.GeneratedAssetPaths.Select(AssetDatabase.LoadMainAssetAtPath)
					.FirstOrDefault(asset => asset is SpineSheetBakeManifest);
				if (manifest != null) EditorGUIUtility.PingObject(manifest);
				EditorUtility.DisplayDialog("烘焙完成", BuildCompletionMessage(result), "确定");
			} else if (result.Status == SpineSheetBakeStatus.Failed) {
				EditorUtility.DisplayDialog("烘焙失败", result.Error, "确定");
			}
		}

		static string BuildCompletionMessage (SpineSheetBakeResult result) {
			string message = string.Format("生成 {0} 帧、{1} 张图集页、{2} 个视频。",
				result.TotalFrames, result.AtlasPages, result.GeneratedVideoPaths.Count);
			if (result.Warnings.Count > 0)
				message += "\n\n" + string.Join("\n", result.Warnings.ToArray());
			return message;
		}

		void RefreshPreview () {
			string animation = request.AnimationNames.FirstOrDefault();
			string skin = request.SkinNames.FirstOrDefault();
			if (request.Source == null || string.IsNullOrEmpty(animation) || string.IsNullOrEmpty(skin)) return;
			DestroyPreview();
			try {
				previewTexture = SpineSheetRenderer.RenderPreview(request.Clone(), animation, skin, previewTime);
			} catch (Exception exception) {
				Debug.LogError("Spine 预览失败：" + exception);
				previewPlaying = false;
			}
			Repaint();
		}

		void OnEditorUpdate () {
			if (!previewPlaying || request.Source == null) return;
			double now = EditorApplication.timeSinceStartup;
			if (now - lastPreviewUpdate < 0.125d) return;
			float duration = GetAnimationDuration(request.AnimationNames.FirstOrDefault());
			if (duration <= 0f) return;
			previewTime = (previewTime + (float)(now - lastPreviewUpdate)) % duration;
			lastPreviewUpdate = now;
			RefreshPreview();
		}

		float GetAnimationDuration (string animationName) {
			if (request.Source == null || string.IsNullOrEmpty(animationName)) return 0f;
			Spine.Animation animation = request.Source.GetSkeletonData(true)?.FindAnimation(animationName);
			return animation == null ? 0f : animation.Duration;
		}

		int EstimateFrameCount () {
			int total = 0;
			foreach (string name in request.AnimationNames) {
				float duration = GetAnimationDuration(name);
				if (request.UseCustomRange) duration = Mathf.Max(0f, request.RangeEnd - request.RangeStart);
				int perSequence = request.SamplingMode == SpineSheetSamplingMode.ExactFrameCount
					? request.ExactFrameCount
					: Mathf.Max(1, Mathf.CeilToInt(duration * request.FramesPerSecond - 0.00001f));
				total += perSequence * request.SkinNames.Count;
			}
			return total;
		}

		bool CanBake () => request.Source != null && request.AnimationNames.Count > 0 && request.SkinNames.Count > 0;

		void ChooseOutputFolder () {
			string absolute = EditorUtility.OpenFolderPanel("选择 Assets 内的输出目录", Application.dataPath, string.Empty);
			if (string.IsNullOrEmpty(absolute)) return;
			absolute = absolute.Replace('\\', '/');
			string assets = Application.dataPath.Replace('\\', '/');
			if (absolute != assets && !absolute.StartsWith(assets + "/", StringComparison.OrdinalIgnoreCase)) {
				EditorUtility.DisplayDialog("目录无效", "输出目录必须位于当前工程 Assets 下。", "确定");
				return;
			}
			request.OutputFolder = "Assets" + absolute.Substring(assets.Length);
		}

		void SavePreset () {
			string path = EditorUtility.SaveFilePanelInProject("保存 Spine 烘焙预设", "SpineSheetBakerPreset", "asset", "选择预设保存位置");
			if (string.IsNullOrEmpty(path)) return;
			SpineSheetBakerPreset asset = CreateInstance<SpineSheetBakerPreset>();
			asset.Settings = request.Clone();
			AssetDatabase.CreateAsset(asset, path);
			AssetDatabase.SaveAssets();
			preset = asset;
			EditorGUIUtility.PingObject(asset);
		}

		void ApplyPreset (SpineSheetBakerPreset asset) {
			if (asset == null || asset.Settings == null) return;
			string ffmpegExecutable = request.FfmpegExecutable;
			request = asset.Settings.Clone();
			request.FfmpegExecutable = string.IsNullOrWhiteSpace(ffmpegExecutable) ? "ffmpeg" : ffmpegExecutable;
			RefreshSourceLists(false);
			DestroyPreview();
		}

		void ResetSettings () {
			SkeletonDataAsset source = request.Source;
			request = new SpineSheetBakeRequest { Source = source };
			RefreshSourceLists(true);
			DestroyPreview();
		}

		void DestroyPreview () {
			if (previewTexture != null) DestroyImmediate(previewTexture);
			previewTexture = null;
		}

		static void SectionHeader (string title) {
			GUILayout.Space(4f);
			EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
		}
	}
}
