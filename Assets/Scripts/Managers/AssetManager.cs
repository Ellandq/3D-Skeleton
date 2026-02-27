using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Utils.SO;
using Object = UnityEngine.Object;

namespace Managers
{
    public class AssetManager : ManagerBase<AssetManager>
    {
        private class AssetEntry
        {
            public AsyncOperationHandle Handle;
            public int ReferenceCount;
        }

        private readonly Dictionary<string, AssetEntry> _loadAssets = new();

        public async Task PreloadSceneAsync(SceneProfile profile)
        {
            foreach (var key in profile.preloadKeys)
            {
                await LoadAsync<Object>(key);
            }
        }

        public async Task<T> LoadAsync<T>(string key) where T : Object
        {
            if (_loadAssets.TryGetValue(key, out var existing))
            {
                existing.ReferenceCount++;
                return (T)existing.Handle.Result;
            }

            var handle = Addressables.LoadAssetAsync<T>(key);

            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded)
                throw new Exception($"Failed to load asset: {key}");

            _loadAssets[key] = new AssetEntry
            {
                Handle = handle,
                ReferenceCount = 1
            };

            return handle.Result;
        }

        public void Release(string key)
        {
            if (!_loadAssets.TryGetValue(key, out var entry))
                return;

            entry.ReferenceCount--;

            if (entry.ReferenceCount > 0)
                return;
            
            Addressables.Release(entry.Handle);
            _loadAssets.Remove(key);
        }

        public void ReleaseInstance(GameObject instance)
        {
            Addressables.ReleaseInstance(instance);
        }

        public bool IsLoaded(string key) => _loadAssets.ContainsKey(key);
    }
}