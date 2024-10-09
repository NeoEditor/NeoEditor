using DynamicPanels;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NeoEditor
{
    public static class Assets
    {
        public static AssetBundle Scene;
        public static AssetBundle Asset;
        public static AssetBundle DynamicPanels;

        public static RenderTexture GameRenderer;
        public static RenderTexture SceneRenderer;

        public static Panel DynamicPanel;
        public static PanelTab DynamicPanelTab;
        public static RectTransform DynamicPanelPreview;

        public static void Load()
        {
            Scene = AssetBundle.LoadFromMemory(
                File.ReadAllBytes(Path.Combine(Main.Entry.Path, "ne_scene"))
            );
            Asset = AssetBundle.LoadFromMemory(
                File.ReadAllBytes(Path.Combine(Main.Entry.Path, "ne_asset"))
            );
            DynamicPanels = AssetBundle.LoadFromMemory(
                File.ReadAllBytes(Path.Combine(Main.Entry.Path, "dp_asset"))
            );

			GameRenderer = Asset.LoadAsset<RenderTexture>("GameRenderer");
            SceneRenderer = Asset.LoadAsset<RenderTexture>("SceneRenderer");

			DynamicPanel = DynamicPanels.LoadAsset<GameObject>("DynamicPanel").GetComponent<Panel>();
			DynamicPanelTab = DynamicPanels.LoadAsset<GameObject>("DynamicPanelTab").GetComponent<PanelTab>();
			DynamicPanelPreview = DynamicPanels.LoadAsset<GameObject>("DynamicPanelPreview").GetComponent<RectTransform>();
		}
    }
}
