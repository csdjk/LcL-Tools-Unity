using System;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using Spine.Unity.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SpineSheetBaker.Editor {
	internal static class SpineSheetRenderer {
		public static Texture2D RenderPreview (SpineSheetBakeRequest request, string animationName, string skinName, float time) {
			if (request == null || request.Source == null) throw new ArgumentException("请选择骨骼数据。");
			Spine.Animation animation = request.Source.GetSkeletonData(true).FindAnimation(animationName);
			if (animation == null) throw new ArgumentException("找不到动画：" + animationName);
			float start = request.UseCustomRange ? request.RangeStart : 0f;
			float end = request.UseCustomRange ? request.RangeEnd : animation.Duration;
			if (start < 0f || end <= start || end > animation.Duration + 0.0001f)
				throw new ArgumentException("预览采样范围无效。");
			float[] boundsTimes = CreatePreviewBoundsTimes(request, start, end);
			using (RenderSession session = new RenderSession(request, animation, skinName)) {
				Rect bounds = ResolveBounds(session, request, boundsTimes);
				Vector2Int frameSize = ResolveFrameSize(request, bounds);
				Rect cameraBounds = ExpandToAspect(bounds, frameSize.x / (float)frameSize.y);
				session.ResetAnimation();
				session.EvaluateTo(Mathf.Clamp(time, start, end));
				return session.Render(cameraBounds, frameSize);
			}
		}

		static float[] CreatePreviewBoundsTimes (SpineSheetBakeRequest request, float start, float end) {
			if (request.SamplingMode == SpineSheetSamplingMode.FramesPerSecond) {
				return SpineSheetBakerApi.CreateSamplePlan("preview", "preview", "preview", end,
					request.FramesPerSecond, request.Loop, start, end).Times;
			}
			int frameCount = Mathf.Max(1, request.ExactFrameCount);
			float[] times = new float[frameCount];
			float denominator = request.Loop ? frameCount : Mathf.Max(1, frameCount - 1);
			for (int i = 0; i < frameCount; i++)
				times[i] = frameCount == 1 ? (request.Loop ? start : end) : Mathf.Lerp(start, end, i / denominator);
			return times;
		}

		public static List<SpineSheetRenderedFrame> RenderAnimation (
			SpineSheetBakeRequest request,
			Spine.Animation animation,
			string skinName,
			SpineSheetSamplePlan plan,
			Func<SpineSheetBakeProgress, bool> progress,
			int progressOffset,
			int progressTotal) {

			using (RenderSession session = new RenderSession(request, animation, skinName)) {
				Rect bounds = ResolveBounds(session, request, plan.Times);
				Vector2Int frameSize = ResolveFrameSize(request, bounds);
				Rect cameraBounds = ExpandToAspect(bounds, frameSize.x / (float)frameSize.y);
				session.ResetAnimation();

				List<SpineSheetRenderedFrame> frames = new List<SpineSheetRenderedFrame>(plan.Times.Length);
				int nonEmptyFrames = 0;
				try {
					for (int i = 0; i < plan.Times.Length; i++) {
						SpineSheetBakeProgress state = new SpineSheetBakeProgress {
							Stage = "渲染",
							Item = animation.Name + " / " + skinName,
							Completed = progressOffset + i,
							Total = progressTotal
						};
						if (progress != null && !progress(state))
							throw new OperationCanceledException();

						session.EvaluateTo(plan.Times[i]);
						Texture2D texture = session.Render(cameraBounds, frameSize);
						if (HasVisiblePixels(texture)) nonEmptyFrames++;
						frames.Add(new SpineSheetRenderedFrame {
							Animation = animation.Name,
							Skin = skinName,
							Index = i,
							Time = plan.Times[i],
							Name = plan.FrameNames[i],
							Texture = texture
						});
					}
				} catch {
					DestroyFrames(frames);
					throw;
				}

				if (nonEmptyFrames == 0) {
					DestroyFrames(frames);
					throw new InvalidOperationException(string.Format("动画 {0} / {1} 的所有采样帧均为空。", animation.Name, skinName));
				}
				return frames;
			}
		}

		public static void DestroyFrames (IEnumerable<SpineSheetRenderedFrame> frames) {
			if (frames == null) return;
			foreach (SpineSheetRenderedFrame frame in frames) {
				if (frame != null && frame.Texture != null) Object.DestroyImmediate(frame.Texture);
			}
		}

		static Rect ResolveBounds (RenderSession session, SpineSheetBakeRequest request, float[] times) {
			Rect result;
			if (request.BoundsMode == SpineSheetBoundsMode.Custom) {
				result = request.CustomBounds;
			} else if (request.BoundsMode == SpineSheetBoundsMode.SetupPose) {
				session.ResetSetupPose();
				result = session.GetCurrentBounds();
			} else {
				session.ResetAnimation();
				bool hasBounds = false;
				result = default(Rect);
				for (int i = 0; i < times.Length; i++) {
					session.EvaluateTo(times[i]);
					Rect current = session.GetCurrentBounds();
					if (!hasBounds) {
						result = current;
						hasBounds = true;
					} else {
						result = Union(result, current);
					}
				}
			}

			if (result.width <= 0f || result.height <= 0f || !IsFinite(result))
				throw new InvalidOperationException("无法从骨架姿势得到有效取景范围。");
			if (request.BoundsPadding > 0f) {
				result.xMin -= request.BoundsPadding;
				result.xMax += request.BoundsPadding;
				result.yMin -= request.BoundsPadding;
				result.yMax += request.BoundsPadding;
			}
			return result;
		}

		static Vector2Int ResolveFrameSize (SpineSheetBakeRequest request, Rect bounds) {
			int width;
			int height;
			if (request.SizingMode == SpineSheetSizingMode.FixedPixels) {
				width = request.FrameWidth;
				height = request.FrameHeight;
			} else {
				width = Mathf.CeilToInt(bounds.width * request.PixelsPerUnit);
				height = Mathf.CeilToInt(bounds.height * request.PixelsPerUnit);
			}
			width = Mathf.Max(1, width);
			height = Mathf.Max(1, height);
			if (width + request.Padding * 2 > request.MaxAtlasSize || height + request.Padding * 2 > request.MaxAtlasSize)
				throw new InvalidOperationException(string.Format("单帧尺寸 {0}x{1} 超过图集最大尺寸 {2}。", width, height, request.MaxAtlasSize));
			return new Vector2Int(width, height);
		}

		static Rect ExpandToAspect (Rect bounds, float targetAspect) {
			float aspect = bounds.width / bounds.height;
			if (aspect < targetAspect) {
				float width = bounds.height * targetAspect;
				bounds.x -= (width - bounds.width) * 0.5f;
				bounds.width = width;
			} else if (aspect > targetAspect) {
				float height = bounds.width / targetAspect;
				bounds.y -= (height - bounds.height) * 0.5f;
				bounds.height = height;
			}
			return bounds;
		}

		static Rect Union (Rect a, Rect b) {
			float xMin = Mathf.Min(a.xMin, b.xMin);
			float yMin = Mathf.Min(a.yMin, b.yMin);
			float xMax = Mathf.Max(a.xMax, b.xMax);
			float yMax = Mathf.Max(a.yMax, b.yMax);
			return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
		}

		static bool IsFinite (Rect rect) {
			return !float.IsNaN(rect.x) && !float.IsInfinity(rect.x) &&
				!float.IsNaN(rect.y) && !float.IsInfinity(rect.y) &&
				!float.IsNaN(rect.width) && !float.IsInfinity(rect.width) &&
				!float.IsNaN(rect.height) && !float.IsInfinity(rect.height);
		}

		static bool HasVisiblePixels (Texture2D texture) {
			Color32[] pixels = texture.GetPixels32();
			for (int i = 0; i < pixels.Length; i++) {
				if (pixels[i].a > 2) return true;
			}
			return false;
		}

		sealed class RenderSession : IDisposable {
			readonly SpineSheetBakeRequest request;
			readonly Spine.Animation animation;
			readonly string skinName;
			readonly PreviewRenderUtility preview;
			readonly SkeletonAnimation skeletonAnimation;
			readonly Camera camera;
			float evaluatedTime;
			float[] boundsVertices;

			public RenderSession (SpineSheetBakeRequest request, Spine.Animation animation, string skinName) {
				this.request = request;
				this.animation = animation;
				this.skinName = skinName;
				skeletonAnimation = EditorInstantiation.InstantiateSkeletonAnimation(request.Source, skinName,
					destroyInvalid: true, useObjectFactory: false);
				if (skeletonAnimation == null) throw new InvalidOperationException("无法实例化 Spine 骨架。");
				skeletonAnimation.gameObject.hideFlags = HideFlags.HideAndDontSave;
				skeletonAnimation.gameObject.layer = 31;
				Renderer renderer = skeletonAnimation.GetComponent<Renderer>();
				if (renderer != null) renderer.enabled = true;

				preview = new PreviewRenderUtility(true);
				preview.AddSingleGO(skeletonAnimation.gameObject);
				camera = preview.camera;
				camera.enabled = false;
				camera.orthographic = true;
				camera.clearFlags = CameraClearFlags.SolidColor;
				camera.backgroundColor = Color.clear;
				camera.cullingMask = 1 << 31;
				camera.nearClipPlane = 0.01f;
				camera.farClipPlane = 100f;
				camera.allowHDR = false;
				camera.allowMSAA = request.Msaa > 1;
				ResetAnimation();
			}

			public void ResetSetupPose () {
				skeletonAnimation.AnimationState.ClearTracks();
				ApplySkinAndScale();
				skeletonAnimation.Skeleton.SetToSetupPose();
				skeletonAnimation.Update(0f);
				skeletonAnimation.LateUpdate();
				evaluatedTime = 0f;
			}

			public void ResetAnimation () {
				ResetSetupPose();
				skeletonAnimation.AnimationState.SetAnimation(0, animation, request.Loop);
				skeletonAnimation.Update(0f);
				skeletonAnimation.LateUpdate();
			}

			void ApplySkinAndScale () {
				SkeletonData data = request.Source.GetSkeletonData(true);
				Skin skin = data.FindSkin(skinName);
				if (skin == null && skinName == "default") skin = data.DefaultSkin;
				if (skin == null) throw new InvalidOperationException("找不到 Skin：" + skinName);
				skeletonAnimation.Skeleton.SetSkin(skin);
				skeletonAnimation.Skeleton.ScaleX = request.FlipX ? -1f : 1f;
				skeletonAnimation.Skeleton.ScaleY = request.FlipY ? -1f : 1f;
			}

			public void EvaluateTo (float targetTime) {
				if (targetTime + 0.00001f < evaluatedTime) ResetAnimation();
				float remaining = Mathf.Max(0f, targetTime - evaluatedTime);
				const float maxStep = 1f / 60f;
				while (remaining > 0.000001f) {
					float step = Mathf.Min(maxStep, remaining);
					skeletonAnimation.Update(step);
					remaining -= step;
				}
				skeletonAnimation.LateUpdate();
				evaluatedTime = targetTime;
			}

			public Rect GetCurrentBounds () {
				float x, y, width, height;
				skeletonAnimation.Skeleton.GetBounds(out x, out y, out width, out height, ref boundsVertices,
					new SkeletonClipping());
				return new Rect(x, y, width, height);
			}

			public Texture2D Render (Rect bounds, Vector2Int size) {
				int supersampling = Mathf.Clamp(request.Supersampling, 1, 4);
				int renderWidth = size.x * supersampling;
				int renderHeight = size.y * supersampling;
				int msaa = NormalizeMsaa(request.Msaa);
				RenderTexture rendered = new RenderTexture(renderWidth, renderHeight, 24,
					RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB) {
					antiAliasing = msaa,
					filterMode = request.FilterMode,
					wrapMode = TextureWrapMode.Clamp,
					name = "SpineSheetBakerFrame"
				};
				rendered.Create();
				RenderTexture downsampled = null;
				RenderTexture previousActive = RenderTexture.active;
				try {
					camera.aspect = size.x / (float)size.y;
					camera.orthographicSize = bounds.height * 0.5f;
					camera.transform.position = new Vector3(bounds.center.x, bounds.center.y, -10f);
					camera.transform.rotation = Quaternion.identity;
					camera.targetTexture = rendered;
					camera.Render();

					RenderTexture source = rendered;
					if (supersampling > 1) {
						downsampled = new RenderTexture(size.x, size.y, 0, RenderTextureFormat.ARGB32,
							RenderTextureReadWrite.sRGB) { filterMode = request.FilterMode };
						downsampled.Create();
						Graphics.Blit(rendered, downsampled);
						source = downsampled;
					}

					RenderTexture.active = source;
					Texture2D texture = new Texture2D(size.x, size.y, TextureFormat.RGBA32, false, false) {
						filterMode = request.FilterMode,
						wrapMode = TextureWrapMode.Clamp
					};
					texture.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0, false);
					texture.Apply(false, false);
					if (request.AlphaMode == SpineSheetAlphaMode.Straight) Unpremultiply(texture);
					return texture;
				} finally {
					camera.targetTexture = null;
					RenderTexture.active = previousActive;
					if (downsampled != null) {
						downsampled.Release();
						Object.DestroyImmediate(downsampled);
					}
					rendered.Release();
					Object.DestroyImmediate(rendered);
				}
			}

			public void Dispose () {
				preview.Cleanup();
			}
		}

		static int NormalizeMsaa (int value) {
			if (value >= 8) return 8;
			if (value >= 4) return 4;
			if (value >= 2) return 2;
			return 1;
		}

		static void Unpremultiply (Texture2D texture) {
			Color32[] pixels = texture.GetPixels32();
			for (int i = 0; i < pixels.Length; i++) {
				Color32 pixel = pixels[i];
				if (pixel.a == 0) {
					pixel.r = pixel.g = pixel.b = 0;
				} else if (pixel.a < 255) {
					float scale = 255f / pixel.a;
					pixel.r = (byte)Mathf.Clamp(Mathf.RoundToInt(pixel.r * scale), 0, 255);
					pixel.g = (byte)Mathf.Clamp(Mathf.RoundToInt(pixel.g * scale), 0, 255);
					pixel.b = (byte)Mathf.Clamp(Mathf.RoundToInt(pixel.b * scale), 0, 255);
				}
				pixels[i] = pixel;
			}
			texture.SetPixels32(pixels);
			texture.Apply(false, false);
		}
	}
}
