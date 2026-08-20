using System.Collections.Generic;
using UnityEngine;

namespace SpineSheetBaker.Editor {
	public sealed class SpineSheetBakeManifest : ScriptableObject {
		[SerializeField] string sourceGuid;
		[SerializeField] string sourcePath;
		[SerializeField] string settingsJson;
		[SerializeField] List<string> generatedAssetPaths = new List<string>();
		[SerializeField] List<SpineSheetBakeFrameRecord> frames = new List<SpineSheetBakeFrameRecord>();

		public string SourceGuid => sourceGuid;
		public string SourcePath => sourcePath;
		public string SettingsJson => settingsJson;
		public IReadOnlyList<string> GeneratedAssetPaths => generatedAssetPaths;
		public IReadOnlyList<SpineSheetBakeFrameRecord> Frames => frames;

		internal void SetData (string guid, string path, SpineSheetBakeRequest request,
			IEnumerable<string> assets, IEnumerable<SpineSheetBakeFrameRecord> frameRecords) {
			sourceGuid = guid;
			sourcePath = path;
			settingsJson = JsonUtility.ToJson(request);
			generatedAssetPaths = new List<string>(assets);
			frames = new List<SpineSheetBakeFrameRecord>(frameRecords);
		}
	}
}
