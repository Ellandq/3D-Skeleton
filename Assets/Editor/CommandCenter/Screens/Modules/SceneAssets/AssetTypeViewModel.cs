using System.Collections.Generic;

namespace Editor.CommandCenter.Screens.Modules.SceneAssets
{
    public class AssetTypeViewModel
    {
        public string address;

        public string displayName;

        public string usesSaveData;

        public bool isSaved;

        public readonly List<ScenePropViewModel> assets = new();
    }
}