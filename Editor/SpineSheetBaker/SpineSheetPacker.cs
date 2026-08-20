using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpineSheetBaker.Editor {
	internal sealed class SpineSheetRenderedFrame {
		public string Animation;
		public string Skin;
		public int Index;
		public float Time;
		public string Name;
		public Texture2D Texture;
	}

	internal sealed class SpineSheetPackedFrame {
		public SpineSheetRenderedFrame Source;
		public RectInt Rect;
	}

	internal sealed class SpineSheetPackedPage {
		public Texture2D Texture;
		public readonly List<SpineSheetPackedFrame> Frames = new List<SpineSheetPackedFrame>();
	}

	/// <summary>
	/// 序列帧装箱器。目标：尽量把所有帧装进同一张图集页。
	/// 策略：帧按（高降序、宽降序、原始顺序）排序后，用 MaxRects 装箱逐页首次适应；
	/// 所有帧同尺寸时退化为等价的网格排布。
	/// </summary>
	internal static class SpineSheetPacker {
		static readonly int[] SinglePageCandidateSizes = { 1024, 2048, 4096, 8192 };

		public static List<SpineSheetPackedPage> Pack (IReadOnlyList<SpineSheetRenderedFrame> frames,
			int maxSize, int padding, int edgeExtrude, bool powerOfTwo, bool topDownLayout = false) {

			if (frames == null || frames.Count == 0)
				throw new ArgumentException("没有可装箱的序列帧。", nameof(frames));
			if (maxSize < 32) throw new ArgumentOutOfRangeException(nameof(maxSize));
			if (padding < 0) throw new ArgumentOutOfRangeException(nameof(padding));
			if (edgeExtrude < 0 || edgeExtrude * 2 > padding)
				throw new ArgumentOutOfRangeException(nameof(edgeExtrude), "Padding 必须至少为边缘扩展的两倍。");
			for (int i = 0; i < frames.Count; i++) {
				Texture2D texture = frames[i] == null ? null : frames[i].Texture;
				if (texture == null) throw new ArgumentException("序列帧纹理为空。", nameof(frames));
				if (texture.width + padding * 2 > maxSize || texture.height + padding * 2 > maxSize)
					throw new InvalidOperationException(string.Format("帧 {0} ({1}x{2}) 超出图集最大尺寸 {3}。",
						frames[i].Name, texture.width, texture.height, maxSize));
			}

			List<SpineSheetRenderedFrame> sorted = SortFramesDescending(frames);
			List<PageLayout> layouts = CreateLayouts(sorted, maxSize, padding);
			List<SpineSheetPackedPage> pages = new List<SpineSheetPackedPage>(layouts.Count);
			foreach (PageLayout layout in layouts)
				pages.Add(BuildPage(layout, padding, edgeExtrude, powerOfTwo, maxSize, topDownLayout));
			return pages;
		}

		/// <summary>
		/// 找出能把全部帧装进单页的最小候选尺寸（1024/2048/4096/8192，且必须大于当前尺寸）；
		/// 找不到返回 0。用于多页结果的可操作提示。
		/// </summary>
		public static int FindSinglePageSize (IReadOnlyList<SpineSheetRenderedFrame> frames,
			int currentMaxSize, int padding) {

			if (frames == null || frames.Count == 0) return 0;
			for (int i = 0; i < frames.Count; i++) {
				Texture2D texture = frames[i] == null ? null : frames[i].Texture;
				if (texture == null) return 0;
			}
			List<SpineSheetRenderedFrame> sorted = SortFramesDescending(frames);
			foreach (int size in SinglePageCandidateSizes) {
				if (size <= currentMaxSize) continue;
				if (FitsInSinglePage(sorted, size, padding)) return size;
			}
			return 0;
		}

		static List<PageLayout> CreateLayouts (List<SpineSheetRenderedFrame> sorted, int maxSize, int padding) {
			List<PageLayout> grid = PackUniformGrid(sorted, maxSize, padding);
			if (grid != null) return grid;

			List<MaxRectsPage> pages = new List<MaxRectsPage>();
			foreach (SpineSheetRenderedFrame frame in sorted) {
				bool placed = false;
				for (int i = 0; i < pages.Count; i++) {
					if (pages[i].TryAdd(frame)) {
						placed = true;
						break;
					}
				}
				if (placed) continue;
				MaxRectsPage page = new MaxRectsPage(maxSize, padding);
				pages.Add(page);
				if (!page.TryAdd(frame))
					throw new InvalidOperationException("无法把序列帧放入空图集页。" + frame.Name);
			}

			List<PageLayout> layouts = new List<PageLayout>(pages.Count);
			foreach (MaxRectsPage page in pages) layouts.Add(page.Layout);
			return layouts;
		}

		static bool FitsInSinglePage (List<SpineSheetRenderedFrame> sorted, int size, int padding) {
			int width = sorted[0].Texture.width;
			int height = sorted[0].Texture.height;
			if (width + padding * 2 > size || height + padding * 2 > size) return false;
			bool uniform = true;
			for (int i = 1; i < sorted.Count; i++) {
				if (sorted[i].Texture.width != width || sorted[i].Texture.height != height) {
					uniform = false;
					break;
				}
			}
			if (uniform) {
				int columns = Mathf.Max(1, (size - padding) / (width + padding));
				int rows = Mathf.Max(1, (size - padding) / (height + padding));
				return sorted.Count <= columns * rows;
			}
			MaxRectsPage page = new MaxRectsPage(size, padding);
			for (int i = 0; i < sorted.Count; i++)
				if (!page.TryAdd(sorted[i])) return false;
			return true;
		}

		static List<PageLayout> PackUniformGrid (List<SpineSheetRenderedFrame> sorted, int maxSize, int padding) {
			int width = sorted[0].Texture.width;
			int height = sorted[0].Texture.height;
			for (int i = 1; i < sorted.Count; i++) {
				if (sorted[i].Texture.width != width || sorted[i].Texture.height != height) return null;
			}

			int cellWidth = width + padding;
			int cellHeight = height + padding;
			int columns = Mathf.Max(1, (maxSize - padding) / cellWidth);
			int rows = Mathf.Max(1, (maxSize - padding) / cellHeight);
			int perPage = columns * rows;

			List<PageLayout> layouts = new List<PageLayout>();
			for (int i = 0; i < sorted.Count; i++) {
				int pageIndex = i / perPage;
				while (layouts.Count <= pageIndex) layouts.Add(new PageLayout());
				int indexInPage = i - pageIndex * perPage;
				RectInt rect = new RectInt(
					padding + (indexInPage % columns) * cellWidth,
					padding + (indexInPage / columns) * cellHeight,
					width, height);
				layouts[pageIndex].Record(sorted[i], rect);
			}
			return layouts;
		}

		static List<SpineSheetRenderedFrame> SortFramesDescending (IReadOnlyList<SpineSheetRenderedFrame> frames) {
			int[] order = new int[frames.Count];
			for (int i = 0; i < order.Length; i++) order[i] = i;
			Array.Sort(order, (a, b) => {
				int comparison = frames[b].Texture.height.CompareTo(frames[a].Texture.height);
				if (comparison != 0) return comparison;
				comparison = frames[b].Texture.width.CompareTo(frames[a].Texture.width);
				if (comparison != 0) return comparison;
				return a.CompareTo(b);
			});
			List<SpineSheetRenderedFrame> sorted = new List<SpineSheetRenderedFrame>(frames.Count);
			foreach (int index in order) sorted.Add(frames[index]);
			return sorted;
		}

		static SpineSheetPackedPage BuildPage (PageLayout layout, int padding, int edgeExtrude,
			bool powerOfTwo, int maxSize, bool topDownLayout) {

			int width = Mathf.Max(1, layout.UsedWidth + padding);
			int height = Mathf.Max(1, layout.UsedHeight + padding);
			if (powerOfTwo) {
				width = Mathf.Min(maxSize, Mathf.NextPowerOfTwo(width));
				height = Mathf.Min(maxSize, Mathf.NextPowerOfTwo(height));
			}

			Texture2D atlas = new Texture2D(width, height, TextureFormat.RGBA32, false, false) {
				name = "SpineSheetPage",
				filterMode = FilterMode.Point,
				wrapMode = TextureWrapMode.Clamp
			};
			Color32[] clear = new Color32[width * height];
			atlas.SetPixels32(clear);

			SpineSheetPackedPage page = new SpineSheetPackedPage { Texture = atlas };
			foreach (PlacedFrame placed in layout.Frames) {
				Color32[] pixels = placed.Frame.Texture.GetPixels32();
				atlas.SetPixels32(placed.Rect.x, placed.Rect.y, placed.Rect.width, placed.Rect.height, pixels);
				if (edgeExtrude > 0) Extrude(atlas, placed.Rect, pixels, edgeExtrude);
				page.Frames.Add(new SpineSheetPackedFrame { Source = placed.Frame, Rect = placed.Rect });
			}
			atlas.Apply(false, false);
			if (topDownLayout) FlipPageVertically(atlas, page);
			return page;
		}

		/// <summary>
		/// 把整页纹理垂直翻转并同步镜像 Sprite 矩形坐标，
		/// 使图集图片中帧序按从左到右、从上到下的阅读顺序排列（仅同尺寸帧保证严格顺序）。
		/// </summary>
		static void FlipPageVertically (Texture2D atlas, SpineSheetPackedPage page) {
			int width = atlas.width;
			int height = atlas.height;
			Color32[] pixels = atlas.GetPixels32();
			Color32[] flipped = new Color32[pixels.Length];
			for (int y = 0; y < height; y++)
				Array.Copy(pixels, y * width, flipped, (height - 1 - y) * width, width);
			atlas.SetPixels32(flipped);
			atlas.Apply(false, false);
			for (int i = 0; i < page.Frames.Count; i++) {
				RectInt rect = page.Frames[i].Rect;
				page.Frames[i].Rect = new RectInt(rect.x, height - rect.yMax, rect.width, rect.height);
			}
		}

		static void Extrude (Texture2D atlas, RectInt rect, Color32[] pixels, int amount) {
			int width = rect.width;
			int height = rect.height;
			for (int y = -amount; y < height + amount; y++) {
				for (int x = -amount; x < width + amount; x++) {
					if (x >= 0 && x < width && y >= 0 && y < height) continue;
					int sourceX = Mathf.Clamp(x, 0, width - 1);
					int sourceY = Mathf.Clamp(y, 0, height - 1);
					atlas.SetPixel(rect.x + x, rect.y + y, pixels[sourceY * width + sourceX]);
				}
			}
		}

		sealed class PageLayout {
			public readonly List<PlacedFrame> Frames = new List<PlacedFrame>();
			public int UsedWidth { get; private set; }
			public int UsedHeight { get; private set; }

			public void Record (SpineSheetRenderedFrame frame, RectInt rect) {
				Frames.Add(new PlacedFrame { Frame = frame, Rect = rect });
				UsedWidth = Mathf.Max(UsedWidth, rect.xMax);
				UsedHeight = Mathf.Max(UsedHeight, rect.yMax);
			}
		}

		/// <summary>
		/// MaxRects 装箱页（Jylänki 算法）。
		/// 坐标语义与旧 shelf 一致：可用区域从 (padding, padding) 到 (maxSize, maxSize)，
		/// 帧占位单元为 (宽+padding, 高+padding)，帧矩形从单元原点开始——
		/// 保证边缘扩展（≤ padding/2）永不越界。
		/// </summary>
		sealed class MaxRectsPage {
			readonly int padding;
			readonly PageLayout layout = new PageLayout();
			readonly List<RectInt> freeRects = new List<RectInt>();

			public MaxRectsPage (int maxSize, int padding) {
				this.padding = padding;
				freeRects.Add(new RectInt(padding, padding, maxSize - padding, maxSize - padding));
			}

			public PageLayout Layout => layout;

			public bool TryAdd (SpineSheetRenderedFrame frame) {
				int cellWidth = frame.Texture.width + padding;
				int cellHeight = frame.Texture.height + padding;
				int bestIndex = -1;
				int bestShortSide = int.MaxValue;
				int bestLongSide = int.MaxValue;
				for (int i = 0; i < freeRects.Count; i++) {
					RectInt free = freeRects[i];
					if (free.width < cellWidth || free.height < cellHeight) continue;
					int leftoverWidth = free.width - cellWidth;
					int leftoverHeight = free.height - cellHeight;
					int shortSide = Mathf.Min(leftoverWidth, leftoverHeight);
					int longSide = Mathf.Max(leftoverWidth, leftoverHeight);
					if (shortSide < bestShortSide || (shortSide == bestShortSide && longSide < bestLongSide)) {
						bestShortSide = shortSide;
						bestLongSide = longSide;
						bestIndex = i;
					}
				}
				if (bestIndex < 0) return false;

				RectInt cell = new RectInt(freeRects[bestIndex].x, freeRects[bestIndex].y, cellWidth, cellHeight);
				PlaceCell(cell);
				layout.Record(frame, new RectInt(cell.x, cell.y, frame.Texture.width, frame.Texture.height));
				return true;
			}

			void PlaceCell (RectInt cell) {
				List<RectInt> additions = new List<RectInt>();
				for (int i = freeRects.Count - 1; i >= 0; i--) {
					RectInt free = freeRects[i];
					if (!Intersects(free, cell)) continue;
					freeRects.RemoveAt(i);
					if (cell.x > free.x)
						additions.Add(new RectInt(free.x, free.y, cell.x - free.x, free.height));
					if (cell.xMax < free.xMax)
						additions.Add(new RectInt(cell.xMax, free.y, free.xMax - cell.xMax, free.height));
					if (cell.y > free.y)
						additions.Add(new RectInt(free.x, free.y, free.width, cell.y - free.y));
					if (cell.yMax < free.yMax)
						additions.Add(new RectInt(free.x, cell.yMax, free.width, free.yMax - cell.yMax));
				}
				foreach (RectInt addition in additions)
					freeRects.Add(addition);
				PruneFreeRects();
			}

			void PruneFreeRects () {
				for (int i = 0; i < freeRects.Count; i++) {
					for (int j = i + 1; j < freeRects.Count; j++) {
						if (Contains(freeRects[j], freeRects[i])) {
							freeRects.RemoveAt(i);
							i--;
							break;
						}
						if (Contains(freeRects[i], freeRects[j])) {
							freeRects.RemoveAt(j);
							j--;
						}
					}
				}
			}

			static bool Intersects (RectInt a, RectInt b) {
				return a.x < b.xMax && b.x < a.xMax && a.y < b.yMax && b.y < a.yMax;
			}

			static bool Contains (RectInt outer, RectInt inner) {
				return outer.x <= inner.x && outer.y <= inner.y &&
					inner.xMax <= outer.xMax && inner.yMax <= outer.yMax;
			}
		}

		sealed class PlacedFrame {
			public SpineSheetRenderedFrame Frame;
			public RectInt Rect;
		}
	}
}
