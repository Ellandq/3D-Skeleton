using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Utils.Contract;
using Utils.Misc;
using Utils.SO.Scene;

namespace Managers
{
    public class AssetManager : ManagerBase<AssetManager>, IAsyncInitializable
    {
        private class AssetEntry
        {
            public AsyncOperationHandle<GameObject> Handle;
            public int ReferenceCount;
        }

        private readonly Dictionary<string, AssetEntry> _loadedAssets = new();
        private readonly Dictionary<GameObject, string> _instances = new();

        public string ProcessName => "Asset Manager";

        public async Task<GameObject> InstantiatePrefabAsync<TEnum>(TEnum enumValue, Transform parent,
            bool enable = true) where TEnum : Enum =>
            await InstantiatePrefabAsync(enumValue.ToString(), parent, enable);
        
        public async Task<GameObject> InstantiatePrefabAsync(
            string key, 
            Transform parent, 
            bool enable = true)
        {
            if (!_loadedAssets.TryGetValue(key, out var entry))
            {
                var handle = Addressables.LoadAssetAsync<GameObject>(key);
                await handle.Task;

                if (handle.Status != AsyncOperationStatus.Succeeded)
                    throw new Exception($"Failed to load asset: {key}");

                entry = new AssetEntry
                {
                    Handle = handle,
                    ReferenceCount = 0
                };
                _loadedAssets[key] = entry;
            }

            entry.ReferenceCount++;

            var prefab = entry.Handle.Result;
            var instance = Instantiate(prefab, parent);
            instance.SetActive(enable);
            _instances[instance] = key;

            return instance;
        }
        
        public async Task<List<GameObject>> InstantiateManyPrefabAsync(
            PropCollection collection, 
            Transform staticParent, 
            Transform dynamicParent,
            Action<int> declareStepsCallBack,
            Action<string> declareStep,
            bool enable = true)
        {
            var assetCount = collection.dynamicProps.Count + collection.propsData.Count;
            
            if (assetCount == 0)
            {
                declareStepsCallBack.Invoke(1);
                declareStep.Invoke($"No assets to load: {collection.assetAddress}");
                return new List<GameObject>();
            }
            
            declareStepsCallBack.Invoke(assetCount);
            
            var key = collection.assetAddress;
            if (!_loadedAssets.TryGetValue(key, out var entry))
            {
                var handle = Addressables.LoadAssetAsync<GameObject>(key);
                await handle.Task;

                if (handle.Status != AsyncOperationStatus.Succeeded)
                    throw new Exception($"Failed to load asset: {key}");

                entry = new AssetEntry
                {
                    Handle = handle,
                    ReferenceCount = 0
                };

                _loadedAssets[key] = entry;
            }

            var instances = new List<GameObject>();
            var prefab = entry.Handle.Result;
            var i = 0;

            foreach (var dynamic in collection.dynamicProps)
            {
                i++;
                declareStep.Invoke($"Loading asset: {key} ({i}/{assetCount}");
                entry.ReferenceCount++;

                var instance = Instantiate(prefab, dynamic.position, dynamic.rotation, dynamicParent);
                instance.SetActive(enable);
                instance.GetComponent<Transform>().localScale = dynamic.scale;
                
                var rb = instance.GetComponent<Rigidbody>();
                rb.mass = dynamic.mass;
                rb.interpolation = dynamic.interpolation;
                rb.collisionDetectionMode = dynamic.collisionDetectionMode;
                rb.linearVelocity = dynamic.velocity;
                rb.angularVelocity = dynamic.angularVelocity;
                rb.useGravity = dynamic.usesGravity;
                rb.isKinematic = dynamic.isKinematic;

                _instances[instance] = key;
                instances.Add(instance);
            }

            foreach (var st in collection.propsData)
            {
                i++;
                declareStep.Invoke($"Loading asset: {key} ({i}/{assetCount}");
                entry.ReferenceCount++;

                var instance = Instantiate(prefab, st.position, st.rotation, staticParent);
                instance.SetActive(enable);
                instance.GetComponent<Transform>().localScale = st.scale;

                _instances[instance] = key;
                instances.Add(instance);
            }

            return instances;
        }

        public bool IsLoaded<TEnum>(TEnum enumValue) where TEnum : Enum
        {
            return _loadedAssets.ContainsKey(enumValue.ToString());
        }

        public async Task InitializeForScene(
            SceneProfile sceneProfile, 
            Action<int> declareSubprocessesCount, 
            Action<int> declareStepsCallBack,
            Action<string> declareStep)
        {
            var collections = sceneProfile.assetData.collections;
            declareSubprocessesCount.Invoke(collections.Count);
            
            var (dynamicRoot, staticRoot) = ScenePropFinder.GetPropTransforms(sceneProfile.name);

            foreach (var c in collections)
            {
                await InstantiateManyPrefabAsync(c, staticRoot, dynamicRoot, declareStepsCallBack, declareStep);
            }
        }
        
        public void ReleaseInstance(GameObject instance)
        {
            if (!_instances.TryGetValue(instance, out var key))
                return;

            if (!_loadedAssets.TryGetValue(key, out var entry))
            {
                _instances.Remove(instance);
                Destroy(instance);
                return;
            }

            Destroy(instance);
            _instances.Remove(instance);

            entry.ReferenceCount--;

            if (entry.ReferenceCount > 0) return;
            Addressables.Release(entry.Handle);
            _loadedAssets.Remove(key);
        }
    }
}