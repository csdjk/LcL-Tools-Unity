using UnityEditor;
using UnityEngine;
namespace vietlabs.fr2
{
    public interface IWindow
    {
        bool WillRepaint { get; set; }
        void Repaint();
        void OnSelectionChange();
    }

    internal interface IRefDraw
    {
        IWindow window { get; }
        int ElementCount();
        bool DrawLayout();
        bool Draw(Rect rect);
    }

    public abstract class FR2_WindowBase : EditorWindow, IWindow
    {
        public bool WillRepaint { get; set; }
        protected bool showFilter, showIgnore;

        //[NonSerialized] protected bool lockSelection;
        //[NonSerialized] internal List<FR2_Asset> Selected;

        public static bool isNoticeIgnore;

        public void AddItemsToMenu(GenericMenu menu)
        {
            FR2_Cache api = FR2_Cache.Api;
            if (api == null) return;

            menu.AddDisabledItem(FR2_GUIContent.FromString("FR2 - v2.5.8"));
            menu.AddSeparator(string.Empty);

            menu.AddItem(FR2_GUIContent.FromString("Enable"), !FR2_SettingExt.disable, () => { FR2_SettingExt.disable = !FR2_SettingExt.disable; });
            menu.AddItem(FR2_GUIContent.FromString("Refresh"), false, () =>
            {
                //FR2_Asset.lastRefreshTS = Time.realtimeSinceStartup;
                Resources.UnloadUnusedAssets();
                EditorUtility.UnloadUnusedAssetsImmediate();
                FR2_Cache.Api.Check4Changes(true);
                FR2_SceneCache.Api.SetDirty();
            });

#if FR2_DEV
            menu.AddItem(FR2_GUIContent.FromString("Refresh Usage"), false, () => FR2_Cache.Api.Check4Usage());
            menu.AddItem(FR2_GUIContent.FromString("Refresh Selected"), false, ()=> FR2_Cache.Api.RefreshSelection());
            menu.AddItem(FR2_GUIContent.FromString("Clear Cache"), false, () => FR2_Cache.Api.Clear());
#endif
        }

        public abstract void OnSelectionChange();
        protected abstract void OnGUI();

#if UNITY_2018_OR_NEWER
        protected void OnSceneChanged(Scene arg0, Scene arg1)
        {
            if (IsFocusingFindInScene || IsFocusingSceneToAsset || IsFocusingSceneInScene)
            {
                OnSelectionChange();
            }
        }
#endif  
        
        protected bool DrawEnable()
        {
            FR2_Cache api = FR2_Cache.Api;
            if (api == null) return false;
            if (!FR2_SettingExt.disable) return true;

            bool isPlayMode = EditorApplication.isPlayingOrWillChangePlaymode;
            string message = isPlayMode
                ? "Find References 2 is disabled in play mode!"
                : "Find References 2 is disabled!";
            
            EditorGUILayout.HelpBox(FR2_GUIContent.From(message, FR2_Icon.Warning.image));
            if (GUILayout.Button(FR2_GUIContent.FromString("Enable")))
            {
                FR2_SettingExt.disable = !FR2_SettingExt.disable;
                Repaint();
            }
            
            return false;
        }

    }
}
