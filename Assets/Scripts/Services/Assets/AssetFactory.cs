using System;
using System.Collections.Generic;
using System.Linq;
using Components.Props;
using Cysharp.Threading.Tasks;
using Model.Data.Scene;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Utils.Props;

namespace Services.Assets
{
    public class AssetFactory : MonoBehaviour
    {
        private class AssetEntry
        {
            public AsyncOperationHandle<GameObject> handle;
            public int refCount;
        }

        private readonly Dictionary<string, AssetEntry> _cache = new();
        private readonly Dictionary<GameObject, string> _instanceMap = new();
        private readonly Dictionary<string, Queue<PropIdentifier>> _pool = new();

        #region Loading

        private async UniTask<GameObject> LoadPrefabAsync(string key)
        {
            if (!_cache.TryGetValue(key, out var entry))
            {
                var handle = Addressables.LoadAssetAsync<GameObject>(key);
                await handle.ToUniTask();

                if (handle.Status != AsyncOperationStatus.Succeeded)
                    throw new Exception($"Failed to load addressable: {key}");

                entry = new AssetEntry
                {
                    handle = handle,
                    refCount = 0
                };

                _cache[key] = entry;
            }

            entry.refCount++;
            return entry.handle.Result;
        }

        #endregion

        #region API

        public async UniTask<PropIdentifier> InstantiateAsync(
            string address,
            Transform parent = null,
            bool active = true)
        {
            if (TryGetFromPool(address, out var pooled))
                return ReuseExisting(address, pooled, parent, active);

            return await CreateNew(address, parent, active);
        }

        public async UniTask<T> InstantiateAsync<T>(
            string address,
            Transform parent = null,
            bool active = true) where T : Component
        {
            var go = await InstantiateAsync(address, parent, active);
            return go.GetComponent<T>();
        }
        
        public async UniTask<T> InstantiateAsync<T>(
            string address,
            PropData propData,
            Transform parent = null,
            bool active = true) where T : Component
        {
            GameObject go;

            if (TryGetFromPool(address, out var pooled))
            {
                go = ReuseExisting(address, propData, pooled, parent).gameObject;
            }
            else
            {
                var ident = await CreateNew(address, propData, parent);
                go = ident.gameObject;
            }

            go.SetActive(active);

            return go.GetComponent<T>();
        }

        public async UniTask<PropIdentifier> InstantiateAsync(
            string address,
            PropData propData,
            Transform parent = null)
        {
            if (TryGetFromPool(address, out var pooled))
                return ReuseExisting(address, propData, pooled, parent);

            return await CreateNew(address, propData, parent);
        }

        public UniTask<PropIdentifier> InstantiateAsync(
            string address,
            PropData propData,
            PropIdentifier existing,
            Transform parent = null)
        {
            if (!existing)
                throw new ArgumentNullException(nameof(existing));

            return UniTask.FromResult(
                ReuseExisting(address, propData, existing, parent)
            );
        }

        public UniTask<PropIdentifier> InstantiateAsync(
            string address,
            PropIdentifier existing,
            Transform parent = null)
        {
            if (!existing)
                throw new ArgumentNullException(nameof(existing));

            return UniTask.FromResult(
                ReuseExisting(address, existing, parent)
            );
        }

        #endregion

        #region Creation

        private async UniTask<PropIdentifier> CreateNew(
            string address,
            Transform parent,
            bool active)
        {
            var prefab = await LoadPrefabAsync(address);

            var instance = Instantiate(prefab, parent);
            instance.SetActive(active);

            _instanceMap[instance] = address;

            return PropIdUtility.GetOrCreateId(instance, address);
        }

        private async UniTask<PropIdentifier> CreateNew(
            string address,
            PropData propData,
            Transform parent)
        {
            var prefab = await LoadPrefabAsync(address);

            var instance = Instantiate(prefab, propData.position, propData.rotation, parent);
            instance.transform.localScale = propData.scale;

            instance.SetActive(true);

            _instanceMap[instance] = address;

            return PropIdUtility.GetOrCreateId(instance, address, propData.id);
        }

        #endregion

        #region Reuse (POOL)

        private bool TryGetFromPool(string address, out PropIdentifier identifier)
        {
            if (_pool.TryGetValue(address, out var queue) && queue.Count > 0)
            {
                identifier = queue.Dequeue();
                return true;
            }

            identifier = null;
            return false;
        }

        private PropIdentifier ReuseExisting(
            string address,
            PropIdentifier existing,
            Transform parent,
            bool active = true)
        {
            var instance = existing.gameObject;

            _instanceMap[instance] = address;

            instance.transform.SetParent(parent);
            instance.SetActive(active);

            return PropIdUtility.GetOrCreateId(instance, address);
        }

        private PropIdentifier ReuseExisting(
            string address,
            PropData data,
            PropIdentifier existing,
            Transform parent)
        {
            var instance = existing.gameObject;

            _instanceMap[instance] = address;

            instance.transform.SetParent(parent);
            instance.transform.SetPositionAndRotation(data.position, data.rotation);
            instance.transform.localScale = data.scale;

            instance.SetActive(true);

            return PropIdUtility.GetOrCreateId(instance, address, data.id);
        }

        #endregion

        #region Pooling

        public void PushToPool(PropIdentifier identifier)
        {
            if (!identifier)
                return;

            var key = identifier.AssetId;

            if (!_pool.TryGetValue(key, out var queue))
            {
                queue = new Queue<PropIdentifier>();
                _pool[key] = queue;
            }

            queue.Enqueue(identifier);

            var go = identifier.gameObject;

            go.SetActive(false);
            go.transform.SetParent(transform);

            _instanceMap.Remove(go);
        }

        #endregion

        #region Release

        public void Release(GameObject instance, bool destroy = true)
        {
            if (!instance)
                return;

            if (!_instanceMap.Remove(instance, out var key))
            {
                if (destroy)
                    Destroy(instance);
                return;
            }

            if (_cache.TryGetValue(key, out var entry))
            {
                entry.refCount--;

                if (entry.refCount <= 0)
                {
                    Addressables.Release(entry.handle);
                    _cache.Remove(key);
                }
            }

            if (destroy)
                Destroy(instance);
        }

        public void ReleaseAll(bool destroy = true)
        {
            foreach (var kv in _instanceMap.Where(kv => destroy && kv.Key))
                Destroy(kv.Key);

            _instanceMap.Clear();

            foreach (var entry in _cache.Values)
                Addressables.Release(entry.handle);

            _cache.Clear();
            _pool.Clear();
        }

        #endregion

        #region Queries

        public bool IsLoaded(string address) => _cache.ContainsKey(address);
        public bool IsInstantiated(GameObject go) => _instanceMap.ContainsKey(go);

        #endregion
    }
}