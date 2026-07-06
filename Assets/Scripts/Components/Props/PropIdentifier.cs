using SaveAndLoad;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;

namespace Components.Props
{
    public class PropIdentifier : MonoBehaviour
    {
        [Header("Core Info")] 
        [SerializeField] private string id;
        [SerializeField] private string assetId;
        public string Id => id;
        public string AssetId => assetId;

        [Header("Component References")] 
        [SerializeField] private Rigidbody rb;
        [SerializeField] private MonoBehaviour saveable;
        public Rigidbody Rigidbody => rb;
        public ISaveable Savable => saveable as ISaveable;

        public void Initialize(string value, string assetPath)
        {
            id = value;
            assetId = assetPath;
            if (TryGetComponent<Rigidbody>(out var comp1)) rb = comp1;
            if (TryGetComponent(out ISaveable comp2)) saveable = comp2 as MonoBehaviour;
        } 
        
#if UNITY_EDITOR 
        private void OnValidate()
        {
            id ??= "";
            assetId ??= "";
            
            if (TryGetComponent<Rigidbody>(out var comp)) rb = comp;
            if (TryGetComponent(out ISaveable component)) saveable = component as MonoBehaviour;
            
            if (!string.IsNullOrWhiteSpace(assetId)) return;
            
            var prefab = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
            if (prefab == null) return;
            
            var assetPath = AssetDatabase.GetAssetPath(prefab);
            if (string.IsNullOrEmpty(assetPath)) return;
            
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null) return;
            
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            var entry = settings.FindAssetEntry(guid);
            if (entry == null) return;
            
            assetId = entry.address;
            
            EditorUtility.SetDirty(this);
        } 
#endif
    }
}