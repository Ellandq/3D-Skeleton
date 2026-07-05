using Managers;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
#endif
using UnityEngine;

namespace Utils.Misc.Props
{
    public class PropIdentifier : MonoBehaviour
    {
        [SerializeField] private string id;
        [SerializeField] private string assetId;


        public string Id => id;
        public string AssetId => assetId;


        public void SetId(string value, string assetPath)
        {
            id = value;
            assetId = assetPath;
        }

        private void OnDestroy()
        {
            if (!AssetManager.Instance)
                return;
            AssetManager.ReleaseInstance(gameObject, false);
        }


#if UNITY_EDITOR
        private void OnValidate()
        {
            id ??= "";
            assetId ??= "";

            if (!string.IsNullOrWhiteSpace(assetId))
                return;

            var prefab = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);

            if (prefab == null)
                return;

            var assetPath = AssetDatabase.GetAssetPath(prefab);

            if (string.IsNullOrEmpty(assetPath))
                return;

            var settings = AddressableAssetSettingsDefaultObject.Settings;

            if (settings == null)
                return;

            var guid = AssetDatabase.AssetPathToGUID(assetPath);

            var entry = settings.FindAssetEntry(guid);

            if (entry == null) return;
            assetId = entry.address;
            EditorUtility.SetDirty(this);
        }
#endif
    }
}