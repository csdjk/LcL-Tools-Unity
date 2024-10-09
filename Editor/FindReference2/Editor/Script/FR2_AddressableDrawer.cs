using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ASMStatus = vietlabs.fr2.FR2_Addressable.ASMStatus;
using ProjectStatus = vietlabs.fr2.FR2_Addressable.ProjectStatus;
using AddressInfo = vietlabs.fr2.FR2_Addressable.AddressInfo;

namespace vietlabs.fr2
{
    internal class FR2_AddressableDrawer : IRefDraw
    {
        const string AUTO_DEPEND_TITLE = "(Auto dependency)";
        
        internal readonly FR2_RefDrawer drawer;
        private bool dirty;
        internal Dictionary<string, FR2_Ref> refs;
        internal readonly Dictionary<string, AddressInfo> map =new Dictionary<string, AddressInfo>();
        internal List<string> groups;
        internal float maxWidth;

        public FR2_AddressableDrawer(IWindow window, Func<FR2_RefDrawer.Sort> getSortMode, Func<FR2_RefDrawer.Mode> getGroupMode)
        {
            this.window = window;
            drawer = new FR2_RefDrawer(window, getSortMode, getGroupMode)
            {
                messageNoRefs = "No Addressable Asset",
                messageEmpty = "No Addressable Asset",
                forceHideDetails = true,
                customGetGroup = GetGroup,
                
                customDrawGroupLabel = DrawGroupLabel,
                beforeItemDraw = BeforeDrawItem,
                afterItemDraw = AfterDrawItem,
            };
            
            dirty = true;
            drawer.SetDirty();
        }
        
        
        
        string GetGroup(FR2_Ref rf)
        {
            return rf.group;
        }

        void DrawGroupLabel(Rect r, string label, int childCount)
        {
            var c = GUI.contentColor;
            if (label == AUTO_DEPEND_TITLE)
            {
                var c1 = c;
                c1.a = 0.5f;
                GUI.contentColor = c1;
            }
            
            GUI.Label(r, FR2_GUIContent.FromString(label), EditorStyles.boldLabel);
            GUI.contentColor = c;
        }

        void BeforeDrawItem(Rect r, FR2_Ref rf)
        {
            string guid = rf.asset.guid;
            if (map.TryGetValue(guid, out var address)) return;
            var c = GUI.contentColor;
            c.a = 0.35f;
            GUI.contentColor = c;
        }

        void AfterDrawItem(Rect r, FR2_Ref rf)
        {
            string guid = rf.asset.guid;
            if (!map.TryGetValue(guid, out var address))
            {
                var c2 = GUI.contentColor;
                c2.a = 1f;
                GUI.contentColor = c2;
                return;
            }

            var c = GUI.contentColor;
            var c1 = c;
            c1.a = 0.5f;
            
            GUI.contentColor = c1;
            {
                r.xMin = r.xMax - maxWidth;
                GUI.Label(r, FR2_GUIContent.FromString(address.address), EditorStyles.miniLabel);    
            }
            GUI.contentColor = c;

        }
        
        public IWindow window { get; set; }


        public int ElementCount()
        {
            return refs?.Count ?? 0;
        }

        public bool Draw(Rect rect)
        {
            if (dirty) RefreshView();
            if (refs == null) return false;

            rect.yMax -= 24f;
            bool result = drawer.Draw(rect);
            
            var btnRect = rect;
            btnRect.xMin = btnRect.xMax - 24f;
            btnRect.yMin = btnRect.yMax;
            btnRect.height = 24f;
            
            if (GUI.Button(btnRect, FR2_Icon.Refresh.image))
            {
                FR2_Addressable.Scan();
                RefreshView();
            }
            
            return result;
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

        private readonly Dictionary<ASMStatus, string> AsmMessage = new Dictionary<ASMStatus, string>()
        {
            { ASMStatus.None, "-" },
            { ASMStatus.AsmNotFound, "Addressable Package not imported!" },
            { ASMStatus.TypeNotFound, "Addressable Classes not found (addressable library code changed?)!" },
            { ASMStatus.FieldNotFound, "Addressable Fields not found (addressable library code changed?)!" },
            { ASMStatus.AsmOK, "-" }
        };
        
        private readonly Dictionary<ProjectStatus, string> ProjectStatusMessage = new Dictionary<ProjectStatus, string>()
        {
            { ProjectStatus.None, "-" },
            { ProjectStatus.NoSettings, "No Addressables Settings found!\nOpen [Window/Asset Management/Addressables/Groups] to create new Addressables Settings!\n \n" },
            { ProjectStatus.NoGroup, "No AssetBundle Group created!" },
            { ProjectStatus.Ok, "-" },
        };



        public void RefreshView()
        {
            if (refs == null) refs = new Dictionary<string, FR2_Ref>();
            refs.Clear();
            
            var addresses = FR2_Addressable.GetAddresses();
            if (FR2_Addressable.asmStatus != ASMStatus.AsmOK)
            {
                drawer.messageNoRefs = AsmMessage[FR2_Addressable.asmStatus];
            } else if (FR2_Addressable.projectStatus != ProjectStatus.Ok)
            {
                drawer.messageNoRefs = ProjectStatusMessage[FR2_Addressable.projectStatus];
            }
            drawer.messageEmpty = drawer.messageNoRefs;
            
            if (addresses == null) addresses = new Dictionary<string, AddressInfo>();
            groups = addresses.Keys.ToList();
            map.Clear();

            var maxLengthGroup = string.Empty;
            foreach (KeyValuePair<string, AddressInfo> kvp in addresses)
            {
                // Debug.Log($"{kvp.Key}:\n" + string.Join("\n", kvp.Value.assetGUIDs));
                foreach (string guid in kvp.Value.assetGUIDs)
                {
                    FR2_Asset asset = FR2_Cache.Api.Get(guid);
                    refs.Add(guid, new FR2_Ref(0, 1, asset, null, null)
                    {
                        isSceneRef = false,
                        group = kvp.Value.bundleGroup
                    });
                    
                    map.Add(guid, kvp.Value);
                    if (maxLengthGroup.Length < kvp.Value.address.Length)
                    {
                        maxLengthGroup = kvp.Value.address;
                    } 
                }
                
                foreach (string guid in kvp.Value.childGUIDs)
                {
                    FR2_Asset asset = FR2_Cache.Api.Get(guid);
                    if (refs.ContainsKey(guid)) continue;
                    
                    refs.Add(guid, new FR2_Ref(0, 1, asset, null, null)
                    {
                        isSceneRef = false,
                        group = kvp.Value.bundleGroup
                    });
                    
                    map.Add(guid, kvp.Value);
                    if (maxLengthGroup.Length < kvp.Value.address.Length)
                    {
                        maxLengthGroup = kvp.Value.address;
                    } 
                }
            }

            maxWidth = EditorStyles.miniLabel.CalcSize(FR2_GUIContent.FromString(maxLengthGroup)).x+16f;

            // Find usage
            var usages = FR2_Ref.FindUsage(map.Keys.ToArray());
            foreach (var kvp in usages)
            {
                if (refs.ContainsKey(kvp.Key)) continue;
                var v = kvp.Value;
                
                // do not take script
                if (v.asset.IsScript) continue;
                if (v.asset.IsExcluded) continue;
                
                refs.Add(kvp.Key, kvp.Value);
                kvp.Value.depth = 1;
                kvp.Value.group = AUTO_DEPEND_TITLE;
            }
            
            dirty = false;
            drawer.SetRefs(refs);
        }
        
        internal void RefreshSort()
        {
            drawer.RefreshSort();
        }
    }
}
