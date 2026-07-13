using UnityEditor;
using UnityEngine;
using UnityEditor.AddressableAssets;

namespace Editor.CommandCenter.Screens.Modules.SceneAssets
{
    public static class AddressableUtility
    {
        public static string GetAddress(Object asset)
        {
            if (!asset)
                return null;


            var settings =
                AddressableAssetSettingsDefaultObject.Settings;


            if (!settings)
                return null;


            var path =
                AssetDatabase.GetAssetPath(asset);


            var guid =
                AssetDatabase.AssetPathToGUID(path);


            var entry =
                settings.FindAssetEntry(guid);


            return entry?.address;
        }
    }
}