using System;
using System.Collections.Generic;
using Spine.Unity;
using UnityEditor;
using UnityEngine;

namespace SpineSheetBaker.Editor {
	public enum SpineSheetSamplingMode { FramesPerSecond, ExactFrameCount }
	public enum SpineSheetBoundsMode { AnimationUnion, SetupPose, Custom }
	public enum SpineSheetSizingMode { PixelsPerUnit, FixedPixels }
	public enum SpineSheetAlphaMode { Straight, Premultiplied }
	public enum SpineSheetVideoFormat { Mp4H264, WebMVP9 }
	public enum SpineSheetVideoSizingMode { FollowAtlas, Custom }
	public enum SpineSheetBakeStatus { Succeeded, Cancelled, Failed }

	[Serializable]
	public sealed class SpineSheetBakeRequest {
		public SkeletonDataAsset Source;
		public List<string> AnimationNames = new List<string>();
		public List<string> SkinNames = new List<string> { "default" };
		public SpineSheetSamplingMode SamplingMode = SpineSheetSamplingMode.FramesPerSecond;
		[Min(1)] public int FramesPerSecond = 30;
		[Min(1)] public int ExactFrameCount = 30;
		public bool Loop;
		public bool UseCustomRange;
		[Min(0f)] public float RangeStart;
		[Min(0f)] public float RangeEnd;
		public SpineSheetBoundsMode BoundsMode = SpineSheetBoundsMode.AnimationUnion;
		public Rect CustomBounds = new Rect(-1f, -1f, 2f, 2f);
		[Min(0f)] public float BoundsPadding;
		public SpineSheetSizingMode SizingMode = SpineSheetSizingMode.PixelsPerUnit;
		[Min(1)] public int PixelsPerUnit = 100;
		[Min(1)] public int FrameWidth = 512;
		[Min(1)] public int FrameHeight = 512;
		public bool FlipX;
		public bool FlipY;
		[Range(1, 4)] public int Supersampling = 2;
		public int Msaa = 4;
		public FilterMode FilterMode = FilterMode.Bilinear;
		public SpineSheetAlphaMode AlphaMode = SpineSheetAlphaMode.Straight;
		[Min(32)] public int MaxAtlasSize = 2048;
		[Min(0)] public int Padding = 2;
		[Min(0)] public int EdgeExtrude = 1;
		public bool PowerOfTwoAtlas;
		/// <summary>默认开启。图集图片中帧按从左到右、从上到下的阅读顺序排列（仅同尺寸帧保证严格顺序）。取消勾选则帧 0 回到左下角、向右排再向上换行。</summary>
		public bool TopDownLayout = true;
		public bool GenerateMipMaps;
		public TextureImporterCompression TextureCompression = TextureImporterCompression.Uncompressed;
		public string OutputFolder = "Assets/SpineBaked";
		public bool GenerateAnimationClips = true;
		public bool GenerateAnimatorController;
		public bool GenerateVideo;
		public SpineSheetVideoFormat VideoFormat = SpineSheetVideoFormat.Mp4H264;
		[Range(0, 51)] public int VideoQuality = 18;
		public Color VideoBackgroundColor = Color.black;
		public SpineSheetVideoSizingMode VideoSizingMode = SpineSheetVideoSizingMode.FollowAtlas;
		[Min(0.1f)] public float VideoScale = 1f;
		[NonSerialized] public string FfmpegExecutable = "ffmpeg";

		public SpineSheetBakeRequest Clone () {
			SpineSheetBakeRequest clone = (SpineSheetBakeRequest)MemberwiseClone();
			clone.AnimationNames = new List<string>(AnimationNames ?? new List<string>());
			clone.SkinNames = new List<string>(SkinNames ?? new List<string>());
			return clone;
		}
	}

	public sealed class SpineSheetSamplePlan {
		public float[] Times { get; internal set; } = Array.Empty<float>();
		public float[] PlaybackTimes { get; internal set; } = Array.Empty<float>();
		public string[] FrameNames { get; internal set; } = Array.Empty<string>();
	}

	public sealed class SpineSheetBakeProgress {
		public string Stage { get; internal set; }
		public string Item { get; internal set; }
		public int Completed { get; internal set; }
		public int Total { get; internal set; }
		public float Normalized => Total <= 0 ? 0f : Mathf.Clamp01(Completed / (float)Total);
	}

	public sealed class SpineSheetBakeResult {
		public SpineSheetBakeStatus Status { get; internal set; }
		public string Error { get; internal set; }
		public int TotalFrames { get; internal set; }
		public int AtlasPages { get; internal set; }
		public List<string> GeneratedAssetPaths { get; } = new List<string>();
		public List<string> GeneratedVideoPaths { get; } = new List<string>();
		public List<string> Warnings { get; } = new List<string>();
		public bool Succeeded => Status == SpineSheetBakeStatus.Succeeded;
	}

	[Serializable]
	public sealed class SpineSheetBakeFrameRecord {
		public string Animation;
		public string Skin;
		public int FrameIndex;
		public float Time;
		public string SpriteName;
		public string AtlasPath;
		public Rect SpriteRect;
	}

}
