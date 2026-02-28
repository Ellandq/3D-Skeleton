using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Managers
{
    public class AssetManager : ManagerBase<AssetManager>
    {
        private class AssetEntry
        {
            public AsyncOperationHandle<GameObject> Handle;
            public int ReferenceCount;
        }

        private readonly Dictionary<string, AssetEntry> _loadedAssets = new();
        private readonly Dictionary<GameObject, string> _instances = new();

        public async Task<GameObject> InstantiatePrefabAsync<TEnum>(TEnum enumValue, Transform parent, bool enable = true) where TEnum : Enum
        {
            var key = enumValue.ToString();

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

        public bool IsLoaded<TEnum>(TEnum enumValue) where TEnum : Enum
        {
            return _loadedAssets.ContainsKey(enumValue.ToString());
        }
    }
}