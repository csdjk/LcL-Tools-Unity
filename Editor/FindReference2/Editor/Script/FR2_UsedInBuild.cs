using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace vietlabs.fr2
{
    internal class FR2_UsedInBuild : IRefDraw
    {
        private readonly FR2_RefDrawer drawer;
        private readonly FR2_TreeUI2.GroupDrawer groupDrawer;

        private bool dirty;
        internal Dictionary<string, FR2_Ref> refs;

        public FR2_UsedInBuild(IWindow window, Func<FR2_RefDrawer.Sort> getSortMode, Func<FR2_RefDrawer.Mode> getGroupMode)
        {
            this.window = window;
            drawer = new FR2_RefDrawer(window, getSortMode, getGroupMode)
            {
                messageNoRefs = "No scene enabled in Build Settings!"
            };

            dirty = true;
            drawer.SetDirty();
        }

        public IWindow window { get; set; }


        public int ElementCount()
        {
            return refs?.Count ?? 0;
        }

        public bool Draw(Rect rect)
        {
            if (dirty) RefreshView();
            return drawer.Draw(rect);
        }

        public bool DrawLayout()
        {
            if (dirty) RefreshView();
            return drawer.DrawLayout();
        }

        public void SetDirty()
        {
            dirty = true;
            drawer.SetDirty();
        }

        public void RefreshView()
        {
            var scenes = new HashSet<string>();

            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene == null) continue;
                if (scene.enabled == false) continue;
                string sce = AssetDatabase.AssetPathToGUID(scene.path);
                if (scenes.Contains(sce)) continue;
                scenes.Add(sce);
            }

            refs = new Dictionary<string, FR2_Ref>();
            var directRefs = FR2_Ref.FindUsage(scenes.ToArray());
            foreach (string scene in scenes)
            {
                if (!directRefs.TryGetValue(scene, out FR2_Ref asset)) continue;
                asset.depth = 1;
            }

            List<FR2_Asset> list = FR2_Cache.Api.AssetList;
            int count = list.Count;

            // Collect assets in Resources / Streaming Assets
            for (var i = 0; i < count; i++)
            {
                FR2_Asset item = list[i];
                if (item.inEditor) continue;
                if (item.IsExcluded) continue;
                if (item.IsFolder) continue;
                if (!item.assetPath.StartsWith("Assets/", StringComparison.Ordinal)) continue;

                if (item.inResources || item.inStreamingAsset || item.inPlugins
                    || !string.IsNullOrEmpty(item.AssetBundleName)
                    || !string.IsNullOrEmpty(item.AtlasName))
                {
                    if (refs.ContainsKey(item.guid)) continue;
                    refs.Add(item.guid, new FR2_Ref(0, 1, item, null));
                }
            }

            // Collect direct references
            foreach (var kvp in directRefs)
            {
                var item = kvp.Value.asset;
                if (item.inEditor) continue;
                if (item.IsExcluded) continue;
                if (!item.assetPath.StartsWith("Assets/", StringComparison.Ordinal)) continue;
                if (refs.ContainsKey(item.guid)) continue;
                refs.Add(item.guid, new FR2_Ref(0, 1, item, null));
            }

            drawer.SetRefs(refs);
            dirty = false;
        }

        internal void RefreshSort()
        {
            drawer.RefreshSort();
        }
    }
}
