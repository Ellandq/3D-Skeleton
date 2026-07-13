using Model.Data.Scene;
using SaveAndLoad;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;
using Utils.Enum;

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
        
        public NamedScene FindScene()
        {
            return (NamedScene)System.Enum.Parse(typeof(NamedScene), gameObject.scene.name);
        }
        
        public void ApplyData(DynamicPropData data)
        {
            transform.SetPositionAndRotation(
                data.position,
                data.rotation);

            transform.localScale = data.scale;

            Savable?.LoadSaveData(data.saveData);

            if (!rb)
                return;

            rb.linearVelocity = data.velocity;
            rb.angularVelocity = data.angularVelocity;

            rb.interpolation = data.interpolation;
            rb.collisionDetectionMode = data.collisionDetectionMode;

            rb.useGravity = data.usesGravity;
            rb.isKinematic = data.isKinematic;
            rb.mass = data.mass;
        }
        
        public bool TryGetDynamicPropData(out DynamicPropData prop)
        {
            prop = null;

            if (!Rigidbody)
                return false;


            prop = new DynamicPropData
            {
                id = Id,

                position = transform.position,
                rotation = transform.rotation,
                scale = transform.localScale,

                saveData = Savable?.GetSaveData(),

                velocity = rb.linearVelocity,
                angularVelocity = rb.angularVelocity,

                interpolation = rb.interpolation,
                collisionDetectionMode = rb.collisionDetectionMode,
                usesGravity = rb.useGravity,
                isKinematic = rb.isKinematic,
                mass = rb.mass
            };

            return true;
        }
    }
}