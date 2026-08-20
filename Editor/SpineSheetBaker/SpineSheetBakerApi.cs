using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Spine;
using Spine.Unity;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SpineSheetBaker.Editor {
	public static class SpineSheetBakerApi {
		static readonly EditorCurveBinding ImageSpriteBinding = EditorCurveBinding.PPtrCurve(string.Empty, typeof(Image), "m_Sprite");

		public static SpineSheetBakeResult Bake (SpineSheetBakeRequest request,
			Func<SpineSheetBakeProgress, bool> progress = null) {

			SpineSheetBakeResult result = new SpineSheetBakeResult { Status = SpineSheetBakeStatus.Failed };
			List<SpineSheetRenderedFrame> renderedFrames = new List<SpineSheetRenderedFrame>();
			List<SpineSheetPackedPage> pages = null;
			string stagingDirectory = null;
			try {
				ValidationData validation = Validate(request);
				List<SequencePlan> sequences = CreateSequencePlans(request, validation);
				int totalFrames = sequences.Sum(sequence => sequence.Plan.Times.Length);
				RenderAndPack(sequences, request, validation, progress, totalFrames,
					renderedFrames, out pages);
				if (pages.Count > 1)
					TryShrinkToSinglePage(sequences, validation, request, progress,
						renderedFrames, pages, result);
				stagingDirectory = CreateStagingDirectory();
				List<StagedPage> stagedPages = StagePages(pages, stagingDirectory, validation.BaseName);
				List<SpineSheetStagedVideo> stagedVideos = request.GenerateVideo
					? StageVideos(request, validation.BaseName, sequences, renderedFrames, stagingDirectory, progress)
					: new List<SpineSheetStagedVideo>();
				CommitAssets(request, validation, sequences, stagedPages, stagedVideos, stagingDirectory, result);
				result.TotalFrames = renderedFrames.Count;
				result.AtlasPages = pages.Count;
				result.Status = SpineSheetBakeStatus.Succeeded;
				return result;
			} catch (OperationCanceledException) {
				result.Status = SpineSheetBakeStatus.Cancelled;
				result.Error = "烘焙已取消，未写入任何新资产。";
				return result;
			} catch (Exception exception) {
				result.Status = SpineSheetBakeStatus.Failed;
				result.Error = exception.Message;
				Debug.LogError("Spine 序列帧烘焙失败：" + exception);
				return result;
			} finally {
				SpineSheetRenderer.DestroyFrames(renderedFrames);
				if (pages != null) {
					foreach (SpineSheetPackedPage page in pages)
						if (page.Texture != null) Object.DestroyImmediate(page.Texture);
				}
				try {
					DeleteStagingDirectory(stagingDirectory);
				} catch (Exception cleanupException) {
					result.Warnings.Add("暂存目录清理失败：" + cleanupException.Message);
					Debug.LogWarning("Spine 序列帧烘焙暂存目录清理失败：" + cleanupException);
				}
			}
		}
		public static SpineSheetSamplePlan CreateSamplePlan (
			string skeletonName,
			string animationName,
			string skinName,
			float animationDuration,
			int framesPerSecond,
			bool loop,
			float rangeStart,
			float rangeEnd) {

			if (animationDuration <= 0f)
				throw new ArgumentOutOfRangeException(nameof(animationDuration), "动画时长必须大于 0。");
			if (framesPerSecond <= 0)
				throw new ArgumentOutOfRangeException(nameof(framesPerSecond), "FPS 必须大于 0。");
			if (rangeStart < 0f || rangeEnd <= rangeStart || rangeEnd > animationDuration + 0.0001f)
				throw new ArgumentOutOfRangeException(nameof(rangeEnd), "采样范围必须位于动画时长内，且结束时间大于开始时间。");

			float span = rangeEnd - rangeStart;
			int intervalCount = Mathf.Max(1, Mathf.CeilToInt(span * framesPerSecond - 0.00001f));
			int frameCount = intervalCount;
			float[] times = new float[frameCount];
			float[] playbackTimes = new float[frameCount];
			string[] frameNames = new string[frameCount];
			string prefix = string.Join("_", new[] {
				SanitizeName(skeletonName),
				SanitizeName(animationName),
				SanitizeName(skinName)
			});

			for (int i = 0; i < frameCount; i++) {
				playbackTimes[i] = i / (float)framesPerSecond;
				times[i] = !loop && i == frameCount - 1 ? rangeEnd : rangeStart + playbackTimes[i];
				frameNames[i] = string.Format("{0}_f{1:D4}", prefix, i);
			}

			return new SpineSheetSamplePlan { Times = times, PlaybackTimes = playbackTimes, FrameNames = frameNames };
		}

		public static string SanitizeName (string value) {
			if (string.IsNullOrWhiteSpace(value)) return "unnamed";

			StringBuilder builder = new StringBuilder(value.Length);
			bool previousWasSeparator = false;
			foreach (char character in value.Trim()) {
				bool valid = char.IsLetterOrDigit(character) || character == '-' || character == '_';
				char output = valid ? character : '_';
				if (output == '_') {
					if (previousWasSeparator) continue;
					previousWasSeparator = true;
				} else {
					previousWasSeparator = false;
				}
				builder.Append(output);
			}

			string result = builder.ToString().Trim('_');
			return result.Length == 0 ? "unnamed" : result;
		}

		static void RenderAndPack (List<SequencePlan> sequences, SpineSheetBakeRequest request,
			ValidationData validation, Func<SpineSheetBakeProgress, bool> progress, int totalFrames,
			List<SpineSheetRenderedFrame> renderedFrames, out List<SpineSheetPackedPage> pages) {

			renderedFrames.Clear();
			int completed = 0;
			foreach (SequencePlan sequence in sequences) {
				List<SpineSheetRenderedFrame> frames = SpineSheetRenderer.RenderAnimation(request,
					sequence.Animation, sequence.Skin, sequence.Plan, progress, completed, totalFrames);
				renderedFrames.AddRange(frames);
				completed += frames.Count;
			}
			if (progress != null && !progress(new SpineSheetBakeProgress {
				Stage = "装箱", Item = "生成分页图集", Completed = totalFrames, Total = totalFrames
			})) throw new OperationCanceledException();

			pages = SpineSheetPacker.Pack(renderedFrames, request.MaxAtlasSize, request.Padding,
				request.EdgeExtrude, request.PowerOfTwoAtlas, request.TopDownLayout);
		}

		static void TryShrinkToSinglePage (List<SequencePlan> sequences, ValidationData validation,
			SpineSheetBakeRequest request, Func<SpineSheetBakeProgress, bool> progress,
			List<SpineSheetRenderedFrame> renderedFrames, List<SpineSheetPackedPage> pages,
			SpineSheetBakeResult result) {

			if (request.SizingMode != SpineSheetSizingMode.PixelsPerUnit) {
				int maxWidth = 0;
				int maxHeight = 0;
				foreach (SpineSheetRenderedFrame frame in renderedFrames) {
					if (frame.Texture.width > maxWidth) maxWidth = frame.Texture.width;
					if (frame.Texture.height > maxHeight) maxHeight = frame.Texture.height;
				}
				result.Warnings.Add(string.Format(
					"帧尺寸固定为 {0}×{1}，内容超过图集最大尺寸 {2}，共生成 {3} 页。请减少帧数或降低 FPS。",
					maxWidth, maxHeight, request.MaxAtlasSize, pages.Count));
				return;
			}

			float scale = ComputeShrinkScale(pages, request.MaxAtlasSize, request.Padding);
			if (scale >= 1f) return;
			int originalPpu = request.PixelsPerUnit;
			int shrunkPpu = Mathf.Max(1, Mathf.FloorToInt(originalPpu * scale));
			if (shrunkPpu >= originalPpu) return;

			SpineSheetBakeRequest shrunkRequest = request.Clone();
			shrunkRequest.PixelsPerUnit = shrunkPpu;

			// 丢弃首次渲染的纹理再二次渲染，避免 GPU 显存翻倍。
			SpineSheetRenderer.DestroyFrames(renderedFrames);
			foreach (SpineSheetPackedPage page in pages)
				if (page.Texture != null) Object.DestroyImmediate(page.Texture);
			pages.Clear();
			renderedFrames.Clear();

			ValidationData shrunkValidation = Validate(shrunkRequest);
			List<SequencePlan> shrunkSequences = CreateSequencePlans(shrunkRequest, shrunkValidation);
			int shrunkTotalFrames = shrunkSequences.Sum(sequence => sequence.Plan.Times.Length);

			List<SpineSheetPackedPage> shrunkPages;
			RenderAndPack(shrunkSequences, shrunkRequest, shrunkValidation, progress,
				shrunkTotalFrames, renderedFrames, out shrunkPages);
			pages.AddRange(shrunkPages);

			if (pages.Count == 1) {
				result.Warnings.Add(string.Format(
					"PixelsPerUnit 自动从 {0} 缩小到 {1} 以把所有帧收纳到一张 {2} 图集。",
					originalPpu, shrunkPpu, request.MaxAtlasSize));
			} else {
				result.Warnings.Add(string.Format(
					"PixelsPerUnit 已缩小到 {1}（原 {0}），但仍生成 {2} 页。请降低帧数或 PixelsPerUnit。",
					originalPpu, shrunkPpu, pages.Count));
			}
		}

		// 估算把所有帧按统一比例线性缩小后能塞进单页的缩放系数。
		// MaxRects 装箱密度约 85%，以此预留缓冲。
		static float ComputeShrinkScale (List<SpineSheetPackedPage> pages, int maxSize, int padding) {
			const float packingEfficiency = 0.85f;
			long totalCellArea = 0;
			int maxCellDim = 0;
			foreach (SpineSheetPackedPage page in pages)
				foreach (SpineSheetPackedFrame packed in page.Frames) {
					int w = packed.Source.Texture.width + padding;
					int h = packed.Source.Texture.height + padding;
					totalCellArea += (long)w * h;
					int dim = w > h ? w : h;
					if (dim > maxCellDim) maxCellDim = dim;
				}
			if (totalCellArea <= 0 || maxCellDim <= 0) return 1f;
			float areaScale = maxSize * Mathf.Sqrt(packingEfficiency) / Mathf.Sqrt((float)totalCellArea);
			float dimScale = (float)maxSize / maxCellDim;
			float scale = areaScale < dimScale ? areaScale : dimScale;
			return scale > 1f ? 1f : scale;
		}

		static ValidationData Validate (SpineSheetBakeRequest request) {
			if (request == null) throw new ArgumentNullException(nameof(request));
			if (request.Source == null) throw new ArgumentException("请选择 SkeletonDataAsset。", nameof(request));
			string sourcePath = AssetDatabase.GetAssetPath(request.Source);
			if (string.IsNullOrEmpty(sourcePath)) throw new ArgumentException("SkeletonDataAsset 必须位于当前 Unity 工程中。", nameof(request));
			SkeletonData skeletonData = request.Source.GetSkeletonData(true);
			if (skeletonData == null) throw new InvalidOperationException("SkeletonDataAsset 无法加载，请先修复 Spine 导入错误。");
			if (request.AnimationNames == null || request.AnimationNames.Count == 0)
				throw new ArgumentException("至少选择一个动画。", nameof(request));
			if (request.SkinNames == null || request.SkinNames.Count == 0)
				throw new ArgumentException("至少选择一个 Skin。", nameof(request));
			if (request.SamplingMode == SpineSheetSamplingMode.FramesPerSecond && request.FramesPerSecond <= 0)
				throw new ArgumentOutOfRangeException(nameof(request.FramesPerSecond));
			if (request.SamplingMode == SpineSheetSamplingMode.ExactFrameCount && request.ExactFrameCount <= 0)
				throw new ArgumentOutOfRangeException(nameof(request.ExactFrameCount));
			if (request.PixelsPerUnit <= 0 || request.FrameWidth <= 0 || request.FrameHeight <= 0)
				throw new ArgumentOutOfRangeException(nameof(request.PixelsPerUnit), "分辨率参数必须大于 0。");
			if (request.MaxAtlasSize < 32) throw new ArgumentOutOfRangeException(nameof(request.MaxAtlasSize));
			if (request.Padding < 0 || request.EdgeExtrude < 0 || request.EdgeExtrude * 2 > request.Padding)
				throw new ArgumentException("Padding 必须非负，且必须至少为边缘扩展的两倍。", nameof(request));
			if (request.Msaa != 1 && request.Msaa != 2 && request.Msaa != 4 && request.Msaa != 8)
				throw new ArgumentException("MSAA 仅支持 1、2、4 或 8。", nameof(request));
			if (request.Supersampling != 1 && request.Supersampling != 2 && request.Supersampling != 4)
				throw new ArgumentException("超采样仅支持 1、2 或 4。", nameof(request));
			SpineSheetVideoEncoder.ValidateConfiguration(request);
			string outputFolder = NormalizeAssetFolder(request.OutputFolder);
			request.OutputFolder = outputFolder;

			HashSet<string> animations = new HashSet<string>(request.AnimationNames, StringComparer.Ordinal);
			if (animations.Count != request.AnimationNames.Count) throw new ArgumentException("动画选择中存在重复名称。", nameof(request));
			foreach (string animationName in animations)
				if (skeletonData.FindAnimation(animationName) == null) throw new ArgumentException("找不到动画：" + animationName, nameof(request));
			HashSet<string> skins = new HashSet<string>(request.SkinNames, StringComparer.Ordinal);
			if (skins.Count != request.SkinNames.Count) throw new ArgumentException("Skin 选择中存在重复名称。", nameof(request));
			foreach (string skinName in skins) {
				Skin skin = skeletonData.FindSkin(skinName);
				if (skin == null && !(skinName == "default" && skeletonData.DefaultSkin != null))
					throw new ArgumentException("找不到 Skin：" + skinName, nameof(request));
			}
			if (request.BoundsMode == SpineSheetBoundsMode.Custom && (request.CustomBounds.width <= 0f || request.CustomBounds.height <= 0f))
				throw new ArgumentException("自定义取景框的宽高必须大于 0。", nameof(request));
			return new ValidationData {
				SkeletonData = skeletonData,
				SourcePath = sourcePath,
				SourceGuid = AssetDatabase.AssetPathToGUID(sourcePath),
				BaseName = SanitizeName(request.Source.name)
			};
		}

		static string NormalizeAssetFolder (string value) {
			string folder = string.IsNullOrWhiteSpace(value) ? "Assets/SpineBaked" : value.Trim().Replace('\\', '/').TrimEnd('/');
			if (folder != "Assets" && !folder.StartsWith("Assets/", StringComparison.Ordinal))
				throw new ArgumentException("输出目录必须位于 Assets 下。", nameof(value));
			if (folder.Split('/').Any(part => part == ".." || part == "."))
				throw new ArgumentException("输出目录不能包含相对路径片段。", nameof(value));
			return folder;
		}

		static List<SequencePlan> CreateSequencePlans (SpineSheetBakeRequest request, ValidationData validation) {
			List<SequencePlan> sequences = new List<SequencePlan>();
			foreach (string animationName in request.AnimationNames) {
				Spine.Animation animation = validation.SkeletonData.FindAnimation(animationName);
				float start = request.UseCustomRange ? request.RangeStart : 0f;
				float end = request.UseCustomRange ? request.RangeEnd : animation.Duration;
				if (start < 0f || end <= start || end > animation.Duration + 0.0001f)
					throw new ArgumentException(string.Format("动画 {0} 的采样范围 [{1}, {2}] 无效，动画时长为 {3}。",
						animationName, start, end, animation.Duration));
				foreach (string skinName in request.SkinNames) {
					SpineSheetSamplePlan plan = request.SamplingMode == SpineSheetSamplingMode.FramesPerSecond
						? CreateSamplePlan(request.Source.name, animationName, skinName, animation.Duration,
							request.FramesPerSecond, request.Loop, start, end)
						: CreateExactSamplePlan(request.Source.name, animationName, skinName, start, end,
							request.ExactFrameCount, request.Loop);
					sequences.Add(new SequencePlan { Animation = animation, Skin = skinName, Plan = plan, Start = start, End = end });
				}
			}
			EnsureUniqueSequenceOutputs(sequences, request.GenerateAnimationClips || request.GenerateVideo);
			return sequences;
		}

		static void EnsureUniqueSequenceOutputs (IEnumerable<SequencePlan> sequences, bool includeClipNames) {
			HashSet<string> frameNames = new HashSet<string>(StringComparer.Ordinal);
			HashSet<string> clipNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (SequencePlan sequence in sequences) {
				foreach (string frameName in sequence.Plan.FrameNames) {
					if (!frameNames.Add(frameName))
						throw new InvalidOperationException("动画或 Skin 名称清洗后产生重复 Sprite 名称：" + frameName);
				}
				if (!includeClipNames) continue;
				string clipName = SanitizeName(sequence.Animation.Name) + "_" + SanitizeName(sequence.Skin);
				if (!clipNames.Add(clipName))
					throw new InvalidOperationException("动画或 Skin 名称清洗后产生重复 Clip 名称：" + clipName);
			}
		}

		static SpineSheetSamplePlan CreateExactSamplePlan (string skeletonName, string animationName, string skinName,
			float start, float end, int frameCount, bool loop) {

			if (frameCount <= 0) throw new ArgumentOutOfRangeException(nameof(frameCount));
			float[] times = new float[frameCount];
			float[] playbackTimes = new float[frameCount];
			string[] names = new string[frameCount];
			string prefix = SanitizeName(skeletonName) + "_" + SanitizeName(animationName) + "_" + SanitizeName(skinName);
			for (int i = 0; i < frameCount; i++) {
				float sourceDenominator = loop ? frameCount : Mathf.Max(1, frameCount - 1);
				times[i] = frameCount == 1 ? (loop ? start : end) : Mathf.Lerp(start, end, i / sourceDenominator);
				playbackTimes[i] = i * (end - start) / frameCount;
				names[i] = string.Format("{0}_f{1:D4}", prefix, i);
			}
			return new SpineSheetSamplePlan { Times = times, PlaybackTimes = playbackTimes, FrameNames = names };
		}

		static string CreateStagingDirectory () {
			string root = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Library", "SpineSheetBaker", "Staging");
			string directory = Path.Combine(root, Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(directory);
			return directory;
		}

		static List<StagedPage> StagePages (IReadOnlyList<SpineSheetPackedPage> pages, string stagingDirectory, string baseName) {
			List<StagedPage> staged = new List<StagedPage>(pages.Count);
			for (int i = 0; i < pages.Count; i++) {
				string fileName = PageFileName(baseName, i);
				string stagingPath = Path.Combine(stagingDirectory, fileName);
				byte[] png = pages[i].Texture.EncodeToPNG();
				if (png == null || png.Length == 0) throw new InvalidOperationException("PNG 编码失败：" + fileName);
				File.WriteAllBytes(stagingPath, png);
				staged.Add(new StagedPage { Index = i, FileName = fileName, StagingPath = stagingPath, Page = pages[i] });
			}
			return staged;
		}

		static List<SpineSheetStagedVideo> StageVideos (SpineSheetBakeRequest request, string baseName,
			IReadOnlyList<SequencePlan> sequences, IReadOnlyList<SpineSheetRenderedFrame> renderedFrames,
			string stagingDirectory, Func<SpineSheetBakeProgress, bool> progress) {

			List<SpineSheetStagedVideo> videos = new List<SpineSheetStagedVideo>(sequences.Count);
			int progressTotal = sequences.Sum(sequence => sequence.Plan.Times.Length) + sequences.Count;
			int progressOffset = 0;
			for (int i = 0; i < sequences.Count; i++) {
				SequencePlan sequence = sequences[i];
				List<SpineSheetRenderedFrame> frames = renderedFrames.Where(frame =>
					string.Equals(frame.Animation, sequence.Animation.Name, StringComparison.Ordinal) &&
					string.Equals(frame.Skin, sequence.Skin, StringComparison.Ordinal)).ToList();
				if (frames.Count != sequence.Plan.Times.Length)
					throw new InvalidOperationException(string.Format("视频序列 {0} / {1} 的帧数不完整：期望 {2}，实际 {3}。",
						sequence.Animation.Name, sequence.Skin, sequence.Plan.Times.Length, frames.Count));
				videos.Add(SpineSheetVideoEncoder.Encode(request, baseName, sequence.Animation.Name, sequence.Skin,
					frames, sequence.End - sequence.Start, stagingDirectory, progress, i, progressOffset, progressTotal));
				progressOffset += frames.Count + 1;
			}
			return videos;
		}

		static void CommitAssets (SpineSheetBakeRequest request, ValidationData validation,
			IReadOnlyList<SequencePlan> sequences, IReadOnlyList<StagedPage> stagedPages,
			IReadOnlyList<SpineSheetStagedVideo> stagedVideos,
			string stagingDirectory, SpineSheetBakeResult result) {

			string projectRoot = Directory.GetParent(Application.dataPath).FullName;
			string absoluteOutput = Path.Combine(projectRoot, request.OutputFolder.Replace('/', Path.DirectorySeparatorChar));
			AssetCommitPlan commitPlan = CreateCommitPlan(request, validation, sequences, stagedPages, stagedVideos, projectRoot);
			List<AssetBackup> backups = BackupAssets(commitPlan.AffectedPaths, stagingDirectory, projectRoot);
			bool outputFolderExisted = Directory.Exists(absoluteOutput);
			try {
				Directory.CreateDirectory(absoluteOutput);
				CommitAssetsUnchecked(request, validation, sequences, stagedPages, stagedVideos, result, commitPlan);
			} catch (Exception commitException) {
				try {
					RollbackAssets(commitPlan.AffectedPaths, backups, request.OutputFolder, outputFolderExisted, projectRoot);
				} catch (Exception rollbackException) {
					throw new AggregateException("资产提交失败，且回滚也失败。原始错误：" + commitException.Message,
						commitException, rollbackException);
				}
				throw;
			}
		}

		static void CommitAssetsUnchecked (SpineSheetBakeRequest request, ValidationData validation,
			IReadOnlyList<SequencePlan> sequences, IReadOnlyList<StagedPage> stagedPages,
			IReadOnlyList<SpineSheetStagedVideo> stagedVideos,
			SpineSheetBakeResult result, AssetCommitPlan commitPlan) {

			string projectRoot = Directory.GetParent(Application.dataPath).FullName;
			List<string> generated = new List<string>();
			List<string> generatedVideos = new List<string>();
			Dictionary<string, Sprite> spritesByName = new Dictionary<string, Sprite>(StringComparer.Ordinal);
			List<SpineSheetBakeFrameRecord> records = new List<SpineSheetBakeFrameRecord>();

			foreach (StagedPage staged in stagedPages) {
				string assetPath = request.OutputFolder + "/" + staged.FileName;
				string absolutePath = Path.Combine(projectRoot, assetPath.Replace('/', Path.DirectorySeparatorChar));
				File.Copy(staged.StagingPath, absolutePath, true);
				AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
				ConfigureAtlasImporter(assetPath, staged.Page, request);
				List<Sprite> importedSprites = AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<Sprite>().ToList();
				HashSet<string> expectedNames = new HashSet<string>(staged.Page.Frames.Select(frame => frame.Source.Name), StringComparer.Ordinal);
				if (importedSprites.Count != expectedNames.Count || importedSprites.Any(sprite => !expectedNames.Contains(sprite.name)))
					throw new InvalidOperationException(string.Format("图集 {0} 的 Sprite 切片导入不完整：期望 {1}，实际 {2}。",
						assetPath, expectedNames.Count, importedSprites.Count));
				foreach (Sprite sprite in importedSprites) {
					if (!spritesByName.TryAdd(sprite.name, sprite))
						throw new InvalidOperationException("导入后出现重复 Sprite 名称：" + sprite.name);
				}
				foreach (SpineSheetPackedFrame frame in staged.Page.Frames) {
					records.Add(new SpineSheetBakeFrameRecord {
						Animation = frame.Source.Animation,
						Skin = frame.Source.Skin,
						FrameIndex = frame.Source.Index,
						Time = frame.Source.Time,
						SpriteName = frame.Source.Name,
						AtlasPath = assetPath,
						SpriteRect = new Rect(frame.Rect.x, frame.Rect.y, frame.Rect.width, frame.Rect.height)
					});
				}
				generated.Add(assetPath);
			}

			foreach (SpineSheetStagedVideo stagedVideo in stagedVideos) {
				string assetPath = request.OutputFolder + "/" + stagedVideo.FileName;
				string absolutePath = Path.Combine(projectRoot, assetPath.Replace('/', Path.DirectorySeparatorChar));
				File.Copy(stagedVideo.StagingPath, absolutePath, true);
				AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
				if (!File.Exists(absolutePath) || new FileInfo(absolutePath).Length == 0)
					throw new InvalidOperationException("导入后的视频文件无效：" + assetPath);
				generated.Add(assetPath);
				generatedVideos.Add(assetPath);
			}
			if (stagedVideos.Count > 0 && request.VideoFormat == SpineSheetVideoFormat.WebMVP9 &&
				Application.platform == RuntimePlatform.WindowsEditor &&
				Application.unityVersion.StartsWith("2021.3", StringComparison.Ordinal)) {
				result.Warnings.Add("WebM/VP9 文件已生成，但 Unity 2021.3 Windows Editor 不支持将其导入为 VideoClip；请使用支持 VP9 Alpha 的外部播放器或目标平台验证播放。");
			}

			List<AnimationClip> clips = new List<AnimationClip>();
			if (request.GenerateAnimationClips) {
				foreach (SequencePlan sequence in sequences) {
					string clipPath = request.OutputFolder + "/" + validation.BaseName + "_" +
						SanitizeName(sequence.Animation.Name) + "_" + SanitizeName(sequence.Skin) + ".anim";
					AnimationClip clip = CreateOrUpdateClip(clipPath, sequence, spritesByName, request);
					clips.Add(clip);
					generated.Add(clipPath);
				}
			}

			if (request.GenerateAnimatorController && clips.Count > 0) {
				string controllerPath = request.OutputFolder + "/" + validation.BaseName + "_Baked.controller";
				CreateOrUpdateController(controllerPath, clips);
				generated.Add(controllerPath);
			}

			string manifestPath = commitPlan.ManifestPath;
			SpineSheetBakeManifest manifest = commitPlan.Manifest;
			if (manifest == null) {
				manifest = ScriptableObject.CreateInstance<SpineSheetBakeManifest>();
				AssetDatabase.CreateAsset(manifest, manifestPath);
			}
			manifest.SetData(validation.SourceGuid, validation.SourcePath, request, generated, records);
			EditorUtility.SetDirty(manifest);
			generated.Add(manifestPath);
			RemoveStaleOwnedAssets(commitPlan.PreviousOwned, generated, request.OutputFolder, manifestPath);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
			result.GeneratedAssetPaths.AddRange(generated);
			result.GeneratedVideoPaths.AddRange(generatedVideos);
		}

		static AssetCommitPlan CreateCommitPlan (SpineSheetBakeRequest request, ValidationData validation,
			IReadOnlyList<SequencePlan> sequences, IReadOnlyList<StagedPage> stagedPages,
			IReadOnlyList<SpineSheetStagedVideo> stagedVideos, string projectRoot) {

			string manifestPath = request.OutputFolder + "/" + validation.BaseName + "_SpineSheetBakeManifest.asset";
			Object existingManifestAsset = AssetDatabase.LoadMainAssetAtPath(manifestPath);
			string absoluteManifestPath = Path.Combine(projectRoot, manifestPath.Replace('/', Path.DirectorySeparatorChar));
			if (existingManifestAsset == null && File.Exists(absoluteManifestPath)) {
				AssetDatabase.ImportAsset(manifestPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
				existingManifestAsset = AssetDatabase.LoadMainAssetAtPath(manifestPath);
				if (existingManifestAsset == null)
					throw new InvalidOperationException("目标清单存在但无法加载，拒绝覆盖：" + manifestPath);
			}
			SpineSheetBakeManifest manifest = existingManifestAsset as SpineSheetBakeManifest;
			if (existingManifestAsset != null && manifest == null)
				throw new InvalidOperationException("目标清单路径已被其他资产占用：" + manifestPath);
			if (manifest != null && !string.Equals(manifest.SourceGuid, validation.SourceGuid, StringComparison.Ordinal))
				throw new InvalidOperationException("目标清单属于另一个 SkeletonDataAsset，拒绝覆盖：" + manifestPath);

			List<string> previousOwned = manifest == null
				? new List<string>()
				: new List<string>(manifest.GeneratedAssetPaths);
			HashSet<string> allowedExisting = new HashSet<string>(previousOwned, StringComparer.OrdinalIgnoreCase) { manifestPath };
			List<string> currentTargets = stagedPages
				.Select(staged => request.OutputFolder + "/" + staged.FileName)
				.ToList();
			currentTargets.AddRange(stagedVideos.Select(video => request.OutputFolder + "/" + video.FileName));
			if (request.GenerateAnimationClips) {
				currentTargets.AddRange(sequences.Select(sequence => request.OutputFolder + "/" + validation.BaseName + "_" +
					SanitizeName(sequence.Animation.Name) + "_" + SanitizeName(sequence.Skin) + ".anim"));
			}
			if (request.GenerateAnimatorController && request.GenerateAnimationClips)
				currentTargets.Add(request.OutputFolder + "/" + validation.BaseName + "_Baked.controller");
			currentTargets.Add(manifestPath);
			if (currentTargets.Distinct(StringComparer.OrdinalIgnoreCase).Count() != currentTargets.Count)
				throw new InvalidOperationException("生成资产的目标路径存在名称冲突，请调整动画或 Skin 名称。");

			foreach (string path in currentTargets) {
				if (AssetExists(path, projectRoot) && !allowedExisting.Contains(path))
					throw new InvalidOperationException("目标路径存在非本工具拥有的资产，拒绝覆盖：" + path);
			}

			HashSet<string> affected = new HashSet<string>(currentTargets, StringComparer.OrdinalIgnoreCase);
			foreach (string path in previousOwned) {
				if (!string.IsNullOrEmpty(path) && path.StartsWith(request.OutputFolder + "/", StringComparison.OrdinalIgnoreCase))
					affected.Add(path);
			}
			return new AssetCommitPlan {
				ManifestPath = manifestPath,
				Manifest = manifest,
				PreviousOwned = previousOwned,
				AffectedPaths = affected.ToList()
			};
		}

		static bool AssetExists (string assetPath, string projectRoot) {
			string absolutePath = Path.Combine(projectRoot, assetPath.Replace('/', Path.DirectorySeparatorChar));
			return File.Exists(absolutePath) || AssetDatabase.LoadMainAssetAtPath(assetPath) != null;
		}

		static List<AssetBackup> BackupAssets (IEnumerable<string> assetPaths, string stagingDirectory, string projectRoot) {
			string backupDirectory = Path.Combine(stagingDirectory, "backup");
			Directory.CreateDirectory(backupDirectory);
			List<AssetBackup> backups = new List<AssetBackup>();
			int index = 0;
			foreach (string assetPath in assetPaths.Distinct(StringComparer.OrdinalIgnoreCase)) {
				string absolutePath = Path.Combine(projectRoot, assetPath.Replace('/', Path.DirectorySeparatorChar));
				AssetBackup backup = new AssetBackup { AssetPath = assetPath, Existed = File.Exists(absolutePath) };
				if (backup.Existed) {
					backup.ContentPath = Path.Combine(backupDirectory, index + ".data");
					File.Copy(absolutePath, backup.ContentPath, true);
					string metaPath = absolutePath + ".meta";
					if (File.Exists(metaPath)) {
						backup.MetaPath = Path.Combine(backupDirectory, index + ".meta");
						File.Copy(metaPath, backup.MetaPath, true);
					}
				}
				backups.Add(backup);
				index++;
			}
			return backups;
		}

		static void RollbackAssets (IEnumerable<string> affectedPaths, IEnumerable<AssetBackup> backups,
			string outputFolder, bool outputFolderExisted, string projectRoot) {

			foreach (string assetPath in affectedPaths.Distinct(StringComparer.OrdinalIgnoreCase)) {
				string absolutePath = Path.Combine(projectRoot, assetPath.Replace('/', Path.DirectorySeparatorChar));
				if (File.Exists(absolutePath)) File.Delete(absolutePath);
				if (File.Exists(absolutePath + ".meta")) File.Delete(absolutePath + ".meta");
			}
			foreach (AssetBackup backup in backups.Where(item => item.Existed)) {
				string absolutePath = Path.Combine(projectRoot, backup.AssetPath.Replace('/', Path.DirectorySeparatorChar));
				Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));
				File.Copy(backup.ContentPath, absolutePath, true);
				if (!string.IsNullOrEmpty(backup.MetaPath)) File.Copy(backup.MetaPath, absolutePath + ".meta", true);
			}
			AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
			if (!outputFolderExisted && AssetDatabase.IsValidFolder(outputFolder) && !AssetDatabase.DeleteAsset(outputFolder))
				throw new IOException("回滚时无法移除新建输出目录：" + outputFolder);
		}

		static void ConfigureAtlasImporter (string path, SpineSheetPackedPage page, SpineSheetBakeRequest request) {
			TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
			if (importer == null) throw new InvalidOperationException("无法取得 TextureImporter：" + path);
			importer.textureType = TextureImporterType.Sprite;
			importer.spriteImportMode = SpriteImportMode.Multiple;
			importer.spritePixelsPerUnit = request.PixelsPerUnit;
			importer.mipmapEnabled = request.GenerateMipMaps;
			importer.filterMode = request.FilterMode;
			importer.wrapMode = TextureWrapMode.Clamp;
			importer.alphaIsTransparency = request.AlphaMode == SpineSheetAlphaMode.Straight;
			importer.textureCompression = request.TextureCompression;
			importer.npotScale = TextureImporterNPOTScale.None;
			importer.maxTextureSize = request.MaxAtlasSize;
			SpriteMetaData[] metadata = new SpriteMetaData[page.Frames.Count];
			for (int i = 0; i < page.Frames.Count; i++) {
				SpineSheetPackedFrame frame = page.Frames[i];
				metadata[i] = new SpriteMetaData {
					name = frame.Source.Name,
					rect = new Rect(frame.Rect.x, frame.Rect.y, frame.Rect.width, frame.Rect.height),
					alignment = (int)SpriteAlignment.Custom,
					pivot = new Vector2(0.5f, 0.5f),
					border = Vector4.zero
				};
			}
			importer.spritesheet = metadata;
			importer.SaveAndReimport();
		}

		static AnimationClip CreateOrUpdateClip (string path, SequencePlan sequence,
			IReadOnlyDictionary<string, Sprite> sprites, SpineSheetBakeRequest request) {

			AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
			if (clip == null) {
				clip = new AnimationClip();
				AssetDatabase.CreateAsset(clip, path);
			} else {
				foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(clip))
					AnimationUtility.SetEditorCurve(clip, binding, null);
				foreach (EditorCurveBinding binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
					AnimationUtility.SetObjectReferenceCurve(clip, binding, null);
			}
			clip.name = Path.GetFileNameWithoutExtension(path);
			clip.frameRate = request.SamplingMode == SpineSheetSamplingMode.FramesPerSecond
				? request.FramesPerSecond
				: Mathf.Max(1f, sequence.Plan.Times.Length / Mathf.Max(0.0001f, sequence.End - sequence.Start));
			ObjectReferenceKeyframe[] keys = new ObjectReferenceKeyframe[sequence.Plan.Times.Length];
			for (int i = 0; i < keys.Length; i++) {
				Sprite sprite;
				if (!sprites.TryGetValue(sequence.Plan.FrameNames[i], out sprite))
					throw new InvalidOperationException("找不到已导入的 Sprite：" + sequence.Plan.FrameNames[i]);
				keys[i] = new ObjectReferenceKeyframe { time = sequence.Plan.PlaybackTimes[i], value = sprite };
			}
			AnimationUtility.SetObjectReferenceCurve(clip, ImageSpriteBinding, keys);
			AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
			settings.loopTime = request.Loop;
			settings.startTime = 0f;
			settings.stopTime = sequence.End - sequence.Start;
			AnimationUtility.SetAnimationClipSettings(clip, settings);
			EditorUtility.SetDirty(clip);
			AssetDatabase.SaveAssets();
			return clip;
		}

		static void CreateOrUpdateController (string path, IReadOnlyList<AnimationClip> clips) {
			AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
			if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(path);
			AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
			foreach (ChildAnimatorState childState in stateMachine.states.ToArray())
				stateMachine.RemoveState(childState.state);
			foreach (AnimationClip clip in clips) {
				AnimatorState state = stateMachine.AddState(clip.name);
				state.motion = clip;
			}
			if (clips.Count > 0) {
				AnimatorState first = stateMachine.states.Select(item => item.state).First(item => item.name == clips[0].name);
				stateMachine.defaultState = first;
			}
			EditorUtility.SetDirty(controller);
		}

		static void RemoveStaleOwnedAssets (IEnumerable<string> previousOwned, IEnumerable<string> current,
			string outputFolder, string manifestPath) {

			HashSet<string> retained = new HashSet<string>(current, StringComparer.OrdinalIgnoreCase) { manifestPath };
			foreach (string path in previousOwned) {
				if (string.IsNullOrEmpty(path) || retained.Contains(path)) continue;
				if (!path.StartsWith(outputFolder + "/", StringComparison.OrdinalIgnoreCase)) continue;
				if (AssetExists(path, Directory.GetParent(Application.dataPath).FullName) && !AssetDatabase.MoveAssetToTrash(path))
					throw new IOException("无法清理过期的工具资产：" + path);
			}
		}

		static string PageFileName (string baseName, int pageIndex) {
			return pageIndex == 0 ? baseName + "_sheet.png" : baseName + "_sheet_" + pageIndex + ".png";
		}

		static void DeleteStagingDirectory (string stagingDirectory) {
			if (string.IsNullOrEmpty(stagingDirectory) || !Directory.Exists(stagingDirectory)) return;
			string full = Path.GetFullPath(stagingDirectory);
			string expectedRoot = Path.GetFullPath(Path.Combine(Directory.GetParent(Application.dataPath).FullName,
				"Library", "SpineSheetBaker", "Staging"));
			if (!full.StartsWith(expectedRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
				throw new InvalidOperationException("拒绝清理意外的暂存目录：" + full);
			Directory.Delete(full, true);
		}

		sealed class ValidationData {
			public SkeletonData SkeletonData;
			public string SourcePath;
			public string SourceGuid;
			public string BaseName;
		}

		sealed class SequencePlan {
			public Spine.Animation Animation;
			public string Skin;
			public SpineSheetSamplePlan Plan;
			public float Start;
			public float End;
		}

		sealed class StagedPage {
			public int Index;
			public string FileName;
			public string StagingPath;
			public SpineSheetPackedPage Page;
		}

		sealed class AssetCommitPlan {
			public string ManifestPath;
			public SpineSheetBakeManifest Manifest;
			public List<string> PreviousOwned;
			public List<string> AffectedPaths;
		}

		sealed class AssetBackup {
			public string AssetPath;
			public bool Existed;
			public string ContentPath;
			public string MetaPath;
		}
	}
}
