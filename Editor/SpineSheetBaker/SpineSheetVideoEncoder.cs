using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SpineSheetBaker.Editor {
	internal sealed class SpineSheetStagedVideo {
		public string FileName;
		public string StagingPath;
	}

	internal static class SpineSheetVideoEncoder {
		public static void ValidateConfiguration (SpineSheetBakeRequest request) {
			if (!request.GenerateVideo) return;
			if (request.VideoQuality < 0 || request.VideoQuality > 51)
				throw new ArgumentOutOfRangeException(nameof(request.VideoQuality), "视频质量 CRF 必须位于 0 到 51。\n数值越小质量越高。");
			if (!Enum.IsDefined(typeof(SpineSheetVideoFormat), request.VideoFormat))
				throw new ArgumentOutOfRangeException(nameof(request.VideoFormat));
			if (request.VideoFormat == SpineSheetVideoFormat.Mp4H264 && request.VideoQuality == 0)
				throw new ArgumentOutOfRangeException(nameof(request.VideoQuality),
					"MP4/H.264 Baseline 不支持 CRF 0；请使用 1 到 51，或选择 WebM/VP9。");
			if (string.IsNullOrWhiteSpace(request.FfmpegExecutable))
				throw new ArgumentException("生成视频需要配置 FFmpeg 可执行文件。", nameof(request.FfmpegExecutable));

			try {
				using (Process process = StartProcess(request.FfmpegExecutable, "-version", false)) {
					if (!process.WaitForExit(5000)) {
						TerminateProcess(process);
						throw new TimeoutException("FFmpeg 版本检查超时。");
					}
					if (process.ExitCode != 0)
						throw new InvalidOperationException("FFmpeg 版本检查失败，退出码：" + process.ExitCode);
				}
			} catch (Exception exception) {
				throw new InvalidOperationException("无法启动 FFmpeg。请安装 FFmpeg、加入 PATH，或在窗口中填写可执行文件路径。", exception);
			}
		}

		public static SpineSheetStagedVideo Encode (
			SpineSheetBakeRequest request,
			string baseName,
			string animationName,
			string skinName,
			IReadOnlyList<SpineSheetRenderedFrame> frames,
			float duration,
			string stagingDirectory,
			Func<SpineSheetBakeProgress, bool> progress,
			int sequenceIndex,
			int progressOffset,
			int progressTotal) {

			if (frames == null || frames.Count == 0) throw new ArgumentException("没有可编码的视频帧。", nameof(frames));
			float frameRate = request.SamplingMode == SpineSheetSamplingMode.FramesPerSecond
				? request.FramesPerSecond
				: frames.Count / Mathf.Max(0.0001f, duration);
			string sequenceName = SpineSheetBakerApi.SanitizeName(baseName) + "_" +
				SpineSheetBakerApi.SanitizeName(animationName) + "_" + SpineSheetBakerApi.SanitizeName(skinName);
			string extension = request.VideoFormat == SpineSheetVideoFormat.Mp4H264 ? ".mp4" : ".webm";
			string fileName = sequenceName + extension;
			string outputPath = Path.Combine(stagingDirectory, fileName);
			string frameDirectory = Path.Combine(stagingDirectory, "video_frames_" + sequenceIndex.ToString("D4"));
			Directory.CreateDirectory(frameDirectory);

			for (int i = 0; i < frames.Count; i++) {
				ReportProgress(progress, animationName + " / " + skinName + "（准备帧 " + (i + 1) + "/" + frames.Count + "）",
					progressOffset + i, progressTotal);
				Texture2D videoFrame = CreateVideoFrame(frames[i].Texture, request);
				try {
					byte[] png = videoFrame.EncodeToPNG();
					if (png == null || png.Length == 0) throw new InvalidOperationException("视频帧 PNG 编码失败：" + frames[i].Name);
					File.WriteAllBytes(Path.Combine(frameDirectory, "frame_" + i.ToString("D6") + ".png"), png);
				} finally {
					Object.DestroyImmediate(videoFrame);
				}
			}

			string pattern = Path.Combine(frameDirectory, "frame_%06d.png");
			int videoWidth = frames[0].Texture.width + (frames[0].Texture.width & 1);
			int videoHeight = frames[0].Texture.height + (frames[0].Texture.height & 1);
			string arguments = BuildArguments(request, pattern, outputPath, frameRate, frames.Count,
				videoWidth, videoHeight);
			RunEncoder(request.FfmpegExecutable, arguments, progress, animationName + " / " + skinName,
				progressOffset + frames.Count, progressTotal);
			if (!File.Exists(outputPath) || new FileInfo(outputPath).Length == 0)
				throw new InvalidOperationException("FFmpeg 未生成有效视频：" + fileName);
			ReportProgress(progress, animationName + " / " + skinName + "（完成）",
				progressOffset + frames.Count + 1, progressTotal);

			return new SpineSheetStagedVideo { FileName = fileName, StagingPath = outputPath };
		}

		static Texture2D CreateVideoFrame (Texture2D source, SpineSheetBakeRequest request) {
			if (source == null) throw new ArgumentNullException(nameof(source));
			bool opaque = request.VideoFormat == SpineSheetVideoFormat.Mp4H264;
			Color32 background = request.VideoBackgroundColor;
			Color32 padding = opaque ? new Color32(background.r, background.g, background.b, 255) : new Color32(0, 0, 0, 0);

			Color32[] input = source.GetPixels32();
			int contentWidth = source.width;
			int contentHeight = source.height;
			if (request.VideoSizingMode == SpineSheetVideoSizingMode.Custom) {
				contentWidth = Mathf.Max(1, Mathf.RoundToInt(source.width * request.VideoScale));
				contentHeight = Mathf.Max(1, Mathf.RoundToInt(source.height * request.VideoScale));
				if (contentWidth != source.width || contentHeight != source.height) {
					input = ResizeBilinear(input, source.width, source.height, contentWidth, contentHeight);
				}
			}

			int width = contentWidth + (contentWidth & 1);
			int height = contentHeight + (contentHeight & 1);
			Color32[] output = new Color32[width * height];
			for (int i = 0; i < output.Length; i++) output[i] = padding;

			for (int y = 0; y < contentHeight; y++) {
				for (int x = 0; x < contentWidth; x++) {
					Color32 straight = ToStraight(input[y * contentWidth + x], request.AlphaMode);
					output[y * width + x] = opaque ? CompositeOpaque(straight, background) : straight;
				}
			}

			Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false, false) {
				name = "SpineSheetVideoFrame",
				filterMode = request.FilterMode,
				wrapMode = TextureWrapMode.Clamp
			};
			texture.SetPixels32(output);
			texture.Apply(false, false);
			return texture;
		}

		static Color32[] ResizeBilinear (Color32[] source, int srcWidth, int srcHeight, int dstWidth, int dstHeight) {
			if (source == null) throw new ArgumentNullException(nameof(source));
			Color32[] destination = new Color32[dstWidth * dstHeight];
			float scaleX = (float)srcWidth / dstWidth;
			float scaleY = (float)srcHeight / dstHeight;
			for (int y = 0; y < dstHeight; y++) {
				float srcY = (y + 0.5f) * scaleY - 0.5f;
				int y0 = Mathf.FloorToInt(srcY);
				int y1 = y0 + 1;
				float fy = srcY - y0;
				y0 = Mathf.Clamp(y0, 0, srcHeight - 1);
				y1 = Mathf.Clamp(y1, 0, srcHeight - 1);
				for (int x = 0; x < dstWidth; x++) {
					float srcX = (x + 0.5f) * scaleX - 0.5f;
					int x0 = Mathf.FloorToInt(srcX);
					int x1 = x0 + 1;
					float fx = srcX - x0;
					x0 = Mathf.Clamp(x0, 0, srcWidth - 1);
					x1 = Mathf.Clamp(x1, 0, srcWidth - 1);
					destination[y * dstWidth + x] = LerpColor(
						LerpColor(source[y0 * srcWidth + x0], source[y0 * srcWidth + x1], fx),
						LerpColor(source[y1 * srcWidth + x0], source[y1 * srcWidth + x1], fx),
						fy);
				}
			}
			return destination;
		}

		static Color32 LerpColor (Color32 a, Color32 b, float t) {
			float oneMinusT = 1f - t;
			return new Color32(
				(byte)Mathf.Clamp(Mathf.RoundToInt(a.r * oneMinusT + b.r * t), 0, 255),
				(byte)Mathf.Clamp(Mathf.RoundToInt(a.g * oneMinusT + b.g * t), 0, 255),
				(byte)Mathf.Clamp(Mathf.RoundToInt(a.b * oneMinusT + b.b * t), 0, 255),
				(byte)Mathf.Clamp(Mathf.RoundToInt(a.a * oneMinusT + b.a * t), 0, 255));
		}

		static Color32 ToStraight (Color32 pixel, SpineSheetAlphaMode alphaMode) {
			if (pixel.a == 0) return new Color32(0, 0, 0, 0);
			if (alphaMode == SpineSheetAlphaMode.Straight || pixel.a == 255) return pixel;
			float scale = 255f / pixel.a;
			return new Color32(
				(byte)Mathf.Clamp(Mathf.RoundToInt(pixel.r * scale), 0, 255),
				(byte)Mathf.Clamp(Mathf.RoundToInt(pixel.g * scale), 0, 255),
				(byte)Mathf.Clamp(Mathf.RoundToInt(pixel.b * scale), 0, 255),
				pixel.a);
		}

		static Color32 CompositeOpaque (Color32 foreground, Color32 background) {
			int inverseAlpha = 255 - foreground.a;
			return new Color32(
				(byte)((foreground.r * foreground.a + background.r * inverseAlpha + 127) / 255),
				(byte)((foreground.g * foreground.a + background.g * inverseAlpha + 127) / 255),
				(byte)((foreground.b * foreground.a + background.b * inverseAlpha + 127) / 255),
				255);
		}

		static string BuildArguments (SpineSheetBakeRequest request, string inputPattern, string outputPath,
			float frameRate, int frameCount, int width, int height) {

			string common = "-y -nostdin -hide_banner -loglevel error -framerate " +
				frameRate.ToString("0.######", CultureInfo.InvariantCulture) + " -start_number 0 -i " + Quote(inputPattern) +
				" -frames:v " + frameCount + " -an ";
			if (request.VideoFormat == SpineSheetVideoFormat.Mp4H264) {
				string level = SelectH264Level(width, height, frameRate);
				return common + "-c:v libx264 -preset medium -crf " + request.VideoQuality +
					" -profile:v baseline -level:v " + level + " -pix_fmt yuv420p" +
					" -color_primaries bt709 -color_trc bt709 -colorspace bt709 -color_range tv" +
					" -movflags +faststart+write_colr " + Quote(outputPath);
			}
			return common + "-c:v libvpx-vp9 -crf " + request.VideoQuality +
				" -b:v 0 -pix_fmt yuva420p -auto-alt-ref 0 -metadata:s:v:0 alpha_mode=1 " + Quote(outputPath);
		}

		static string SelectH264Level (int width, int height, float frameRate) {
			long macroblockWidth = (width + 15L) / 16L;
			long macroblockHeight = (height + 15L) / 16L;
			long macroblocksPerFrame = macroblockWidth * macroblockHeight;
			double macroblocksPerSecond = macroblocksPerFrame * frameRate;
			if (FitsH264Level(macroblockWidth, macroblockHeight, macroblocksPerFrame, macroblocksPerSecond,
				3600, 108000, 169)) return "3.1";
			if (FitsH264Level(macroblockWidth, macroblockHeight, macroblocksPerFrame, macroblocksPerSecond,
				5120, 216000, 202)) return "3.2";
			if (FitsH264Level(macroblockWidth, macroblockHeight, macroblocksPerFrame, macroblocksPerSecond,
				8192, 245760, 256)) return "4.1";
			if (FitsH264Level(macroblockWidth, macroblockHeight, macroblocksPerFrame, macroblocksPerSecond,
				8704, 522240, 263)) return "4.2";
			if (FitsH264Level(macroblockWidth, macroblockHeight, macroblocksPerFrame, macroblocksPerSecond,
				22080, 589824, 420)) return "5.0";
			if (FitsH264Level(macroblockWidth, macroblockHeight, macroblocksPerFrame, macroblocksPerSecond,
				36864, 983040, 543)) return "5.1";
			if (FitsH264Level(macroblockWidth, macroblockHeight, macroblocksPerFrame, macroblocksPerSecond,
				36864, 2073600, 543)) return "5.2";
			if (FitsH264Level(macroblockWidth, macroblockHeight, macroblocksPerFrame, macroblocksPerSecond,
				139264, 4177920, 1055)) return "6.0";
			if (FitsH264Level(macroblockWidth, macroblockHeight, macroblocksPerFrame, macroblocksPerSecond,
				139264, 8355840, 1055)) return "6.1";
			if (FitsH264Level(macroblockWidth, macroblockHeight, macroblocksPerFrame, macroblocksPerSecond,
				139264, 16711680, 1055)) return "6.2";
			throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
				"MP4/H.264 帧尺寸 {0}x{1} @ {2:0.###} FPS 超出 Level 6.2 限制；请减小尺寸或帧率。",
				width, height, frameRate));
		}

		static bool FitsH264Level (long macroblockWidth, long macroblockHeight, long macroblocksPerFrame,
			double macroblocksPerSecond, long maxFrameMacroblocks, long maxMacroblocksPerSecond,
			long maxDimensionMacroblocks) {

			return macroblocksPerFrame <= maxFrameMacroblocks &&
				macroblocksPerSecond <= maxMacroblocksPerSecond &&
				macroblockWidth <= maxDimensionMacroblocks && macroblockHeight <= maxDimensionMacroblocks;
		}

		static void RunEncoder (string executable, string arguments, Func<SpineSheetBakeProgress, bool> progress,
			string item, int completed, int total) {

			StringBuilder errors = new StringBuilder();
			using (Process process = StartProcess(executable, arguments, true)) {
				process.ErrorDataReceived += (sender, eventArgs) => {
					if (!string.IsNullOrEmpty(eventArgs.Data)) lock (errors) errors.AppendLine(eventArgs.Data);
				};
				process.BeginErrorReadLine();
				try {
					ReportProgress(progress, item, completed, total);
					while (!process.WaitForExit(100)) ReportProgress(progress, item, completed, total);
					process.WaitForExit();
				} catch (Exception workException) {
					try {
						TerminateProcess(process);
					} catch (Exception terminateException) {
						throw new AggregateException("FFmpeg 工作被中断，但进程未能安全终止。",
							workException, terminateException);
					}
					throw;
				}
				if (process.ExitCode != 0) {
					string detail;
					lock (errors) detail = errors.ToString().Trim();
					throw new InvalidOperationException("FFmpeg 视频编码失败（退出码 " + process.ExitCode + "）：" + detail);
				}
			}
		}

		static Process StartProcess (string executable, string arguments, bool redirectError) {
			ProcessStartInfo startInfo = new ProcessStartInfo {
				FileName = executable,
				Arguments = arguments,
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardError = redirectError,
				RedirectStandardOutput = false
			};
			Process process = new Process { StartInfo = startInfo, EnableRaisingEvents = false };
			if (!process.Start()) throw new InvalidOperationException("无法启动 FFmpeg 进程。");
			return process;
		}

		static void ReportProgress (Func<SpineSheetBakeProgress, bool> progress, string item, int completed, int total) {
			if (progress != null && !progress(new SpineSheetBakeProgress {
				Stage = "视频编码",
				Item = item,
				Completed = completed,
				Total = total
			})) throw new OperationCanceledException();
		}

		static string Quote (string value) {
			return "\"" + value.Replace("\"", "\\\"") + "\"";
		}

		static void TerminateProcess (Process process) {
			if (process == null || process.HasExited) return;
			process.Kill();
			if (!process.WaitForExit(5000))
				throw new TimeoutException("终止 FFmpeg 进程超时。");
		}
	}
}
