#nullable enable 
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using SaveAndLoad;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Utils.Contract;
using Utils.Data.Save;
using Utils.Data.Scene;
using Utils.Enum;
using Utils.Misc.Props;

namespace Managers
{
    public class AssetManager : ManagerBase<AssetManager>, IAsyncInitializable
    {
        private class AssetEntry
        {
            public AsyncOperationHandle<GameObject> handle;
            public int referenceCount;
        }

        private readonly Dictionary<string, AssetEntry> _loadedAssets = new();
        private readonly Dictionary<GameObject, string> _instances = new();
        private Dictionary<NamedScene, bool> sceneAssetLoaded = new();
        public string ProcessName => "Asset Manager";

        public async UniTask InitializeForScene(SceneProfile sceneProfile, Action<int> declareSubprocessesCount,
            Action<int> declareStepsCallBack, Action<string> declareStep)
        {
            var collections = sceneProfile.assetData.collections;
            declareSubprocessesCount(collections.Count);
            var (dynamicRoot, staticRoot) = ScenePropFinder.GetPropTransforms(sceneProfile.name);
            foreach (var c in collections)
            {
                await InstantiateManyPrefabAsync(c, staticRoot, dynamicRoot, declareStepsCallBack, declareStep);
            }
        }

        public static async UniTask<GameObject> InstantiatePrefabAsync<TEnum>(TEnum enumValue, Transform parent,
            bool enable = true) where TEnum : Enum =>
            await Instance.InstantiatePrefabAsync(enumValue.ToString(), parent, enable);

        private async UniTask<GameObject> InstantiatePrefabAsync(string key, Transform parent, bool enable = true)
        {
            if (!_loadedAssets.TryGetValue(key, out var entry))
            {
                var handle = Addressables.LoadAssetAsync<GameObject>(key);
                await handle.ToUniTask();
                if (handle.Status != AsyncOperationStatus.Succeeded)
                    throw new Exception($"Failed to load asset: {key}");
                entry = new AssetEntry { handle = handle, referenceCount = 0 };
                _loadedAssets[key] = entry;
            }

            entry.referenceCount++;
            var prefab = entry.handle.Result;
            var instance = Instantiate(prefab, parent);
            PropIdUtility.GetOrCreateId(instance, key);
            instance.SetActive(enable);
            _instances[instance] = key;
            return instance;
        }

        public async UniTask<List<GameObject>> InstantiateManyPrefabAsync(PropCollection collection,
            Transform staticParent, Transform dynamicParent, Action<int> declareStepsCallBack,
            Action<string> declareStep, bool enable = true)
        {
            var assetCount = collection.dynamicProps.Count + collection.propsData.Count;
            if (assetCount == 0)
            {
                declareStepsCallBack(1);
                declareStep($"No assets to load: {collection.assetAddress}");
                return new List<GameObject>();
            }

            declareStepsCallBack(assetCount);
            var key = collection.assetAddress;
            if (!_loadedAssets.TryGetValue(key, out var entry))
            {
                var handle = Addressables.LoadAssetAsync<GameObject>(key);
                await handle.ToUniTask();
                if (handle.Status != AsyncOperationStatus.Succeeded)
                    throw new Exception($"Failed to load asset: {key}");
                entry = new AssetEntry { handle = handle, referenceCount = 0 };
                _loadedAssets[key] = entry;
            }

            var prefab = entry.handle.Result;
            var instances = new List<GameObject>();
            var i = 0;
            foreach (var dynamic in collection.dynamicProps)
            {
                i++;
                declareStep($"Loading asset: {key} ({i}/{assetCount})");
                entry.referenceCount++;
                var instance = Instantiate(prefab, dynamic.position, dynamic.rotation, dynamicParent);
                instance.SetActive(enable);
                instance.transform.localScale = dynamic.scale;
                var rb = instance.GetComponent<Rigidbody>();
                if (rb)
                {
                    rb.mass = dynamic.mass;
                    rb.interpolation = dynamic.interpolation;
                    rb.collisionDetectionMode = dynamic.collisionDetectionMode;
                    rb.linearVelocity = dynamic.velocity;
                    rb.angularVelocity = dynamic.angularVelocity;
                    rb.useGravity = dynamic.usesGravity;
                    rb.isKinematic = dynamic.isKinematic;
                }

                var identifier = instance.AddComponent<PropIdentifier>();
                identifier.SetId(dynamic.id, key);
                if (!string.IsNullOrEmpty(dynamic.saveData))
                {
                    var saveable = instance.GetComponents<MonoBehaviour>().OfType<ISaveable>().FirstOrDefault();
                    saveable?.LoadSaveData(dynamic.saveData);
                }

                _instances[instance] = key;
                instances.Add(instance);
            }

            foreach (var st in collection.propsData)
            {
                i++;
                declareStep($"Loading asset: {key} ({i}/{assetCount})");
                entry.referenceCount++;
                var instance = UnityEngine.Object.Instantiate(prefab, st.position, st.rotation, staticParent);
                instance.SetActive(enable);
                instance.transform.localScale = st.scale;
                var identifier = instance.AddComponent<PropIdentifier>();
                identifier.SetId(st.id, key);
                if (!string.IsNullOrEmpty(st.saveData))
                {
                    var saveable = instance.GetComponents<MonoBehaviour>().OfType<ISaveable>().FirstOrDefault();
                    saveable?.LoadSaveData(st.saveData);
                }

                _instances[instance] = key;
                instances.Add(instance);
            }

            return instances;
        }

        public static async UniTask<List<SceneSaveData>> CaptureSceneSnapshotAsync(List<NamedScene> scenes)
        {
            await UniTask.WaitForFixedUpdate();
            return scenes.Select(GetSceneAssetDataChangeListInternal).ToList();
        }

        public static SceneSaveData GetSceneAssetDataChangeListInternal(NamedScene namedScene)
        {
            var (dynamicRoot, _) = ScenePropFinder.GetPropTransforms(namedScene.ToString());
            var goDict = new Dictionary<string, Dictionary<string, (Transform tf, Rigidbody rb, string? data)>>();
            for (var i = 0; i < dynamicRoot.childCount; i++)
            {
                var tf = dynamicRoot.GetChild(i);
                var rb = tf.GetComponent<Rigidbody>();
                var identifier = tf.GetComponent<PropIdentifier>();
                if (!identifier) continue;
                var data = tf.GetComponents<MonoBehaviour>().OfType<ISaveable>().FirstOrDefault()?.GetSaveData();
                if (!goDict.TryGetValue(identifier.AssetId, out var assetDict))
                {
                    assetDict = new Dictionary<string, (Transform, Rigidbody, string?)>();
                    goDict.Add(identifier.AssetId, assetDict);
                }

                assetDict[identifier.Id] = (tf, rb, data);
            }

            var propDict = GameLoader.GetSceneProfile(namedScene).GetCollectionDictionary();
            var result = new SceneSaveData { scene = namedScene, collections = new List<SavedProps>() };
            var allAssetIds = new HashSet<string>(propDict.Keys);
            allAssetIds.UnionWith(goDict.Keys);
            foreach (var assetId in allAssetIds)
            {
                propDict.TryGetValue(assetId, out var authored);
                goDict.TryGetValue(assetId, out var runtime);
                var authoredDict = authored ?? new Dictionary<string, DynamicPropData>();
                var runtimeDict = runtime ?? new Dictionary<string, (Transform tf, Rigidbody rb, string? data)>();
                var allIds = new HashSet<string>(authoredDict.Keys);
                allIds.UnionWith(runtimeDict.Keys);
                var changes = new List<(DynamicPropData data, PropSceneStatus status)>();
                foreach (var id in allIds)
                {
                    authoredDict.TryGetValue(id, out var a);
                    runtimeDict.TryGetValue(id, out var r);
                    if (a != null && !r.tf)
                    {
                        changes.Add((a, PropSceneStatus.Removed));
                        continue;
                    }

                    if (a == null && r.tf)
                    {
                        changes.Add((BuildRuntime(assetId, id, r), PropSceneStatus.Added));
                        continue;
                    }

                    if (a == null || !r.tf) continue;
                    if (HasChanged(a, r.tf, r.rb, r.data))
                    {
                        changes.Add((BuildRuntime(assetId, id, r), PropSceneStatus.Changed));
                    }
                }

                if (changes.Count > 0)
                {
                    result.collections.Add(new SavedProps { assetAddress = assetId, dynamicProps = changes });
                }
            }

            return result;
        }

        private static bool HasChanged(DynamicPropData a, Transform tf, Rigidbody rb, string? saveData)
        {
            if (a.position != tf.position || a.rotation != tf.rotation || a.scale != tf.localScale ||
                a.saveData != saveData) return true;
            if (!rb) return false;
            return a.velocity != rb.linearVelocity || a.angularVelocity != rb.angularVelocity ||
                   !Mathf.Approximately(a.mass, rb.mass) || a.usesGravity != rb.useGravity ||
                   a.isKinematic != rb.isKinematic;
        }

        private static DynamicPropData BuildRuntime(string assetId, string id,
            (Transform tf, Rigidbody rb, string? data) r)
        {
            var rbComp = r.rb;
            return new DynamicPropData
            {
                id = id, position = r.tf.position, rotation = r.tf.rotation, scale = r.tf.localScale,
                mass = rbComp ? rbComp.mass : 1f,
                interpolation = rbComp ? rbComp.interpolation : RigidbodyInterpolation.None,
                collisionDetectionMode = rbComp ? rbComp.collisionDetectionMode : CollisionDetectionMode.Discrete,
                velocity = rbComp ? rbComp.linearVelocity : Vector3.zero,
                angularVelocity = rbComp ? rbComp.angularVelocity : Vector3.zero,
                usesGravity = rbComp && rbComp.useGravity, isKinematic = rbComp && rbComp.isKinematic, saveData = r.data
            };
        }

        public static void ReleaseInstance(GameObject instance, bool destroy) =>
            Instance.ReleaseInstanceInternal(instance, destroy);

        private void ReleaseInstanceInternal(GameObject instance, bool destroy)
        {
            if (!_instances.TryGetValue(instance, out var key)) return;
            if (!_loadedAssets.TryGetValue(key, out var entry))
            {
                _instances.Remove(instance);
                if (destroy) Destroy(instance);
                return;
            }

            if (destroy) Destroy(instance);
            _instances.Remove(instance);
            entry.referenceCount--;
            if (entry.referenceCount > 0) return;
            Addressables.Release(entry.handle);
            _loadedAssets.Remove(key);
        }

        public bool IsLoaded<TEnum>(TEnum enumValue) where TEnum : Enum =>
            _loadedAssets.ContainsKey(enumValue.ToString());
    }
}