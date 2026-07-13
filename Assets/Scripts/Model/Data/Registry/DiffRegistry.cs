using System;
using System.Collections.Generic;
using System.Linq;
using Components.Props;
using Managers;
using Model.Data.Save;
using Model.Data.Scene;
using Model.SO.Scene;
using SaveAndLoad;
using Utils.Enum;
using Utils.Props;

namespace Model.Data.Registry
{
    public class DiffRegistry
    {
        private sealed class AddedInfo
        {
            public NamedScene scene;
            public string assetAddress;
            public DynamicPropData data;
        }
        
        private sealed class BaselineInfo
        {
            public NamedScene scene;
            public string assetAddress;
            public DynamicPropData data;
        }
        
        // BASELINE
        private readonly Dictionary<string, BaselineInfo> _baseline = new();
        
        // CHANGELOG
        private readonly Dictionary<string, DynamicPropData> _modified = new();
        private readonly HashSet<string> _removed = new();
        private readonly Dictionary<NamedScene, Dictionary<string, List<DynamicPropData>>> _added = new();
        
        // LOOKUP UTILITY
        private readonly Dictionary<string, AddedInfo> _addedLookup = new();
        private readonly Dictionary<string, (NamedScene scene, string assetAddress)> _propLookup = new();
        
        public DiffRegistry()
        {
            AssetManager.Registry.Registered += OnRegistered;
            AssetManager.Registry.Unregistered += OnUnregistered;

            SceneManager.OnSceneLoaded += OnSceneLoadedListener;
            SceneManager.OnSceneDeloaded += OnSceneDeloadedListener;
        }

        #region REGISTRATION

        private void OnRegistered(PropIdentifier identifier)
        {
            if (!identifier.TryGetDynamicPropData(out var current))
                return;

            _removed.Remove(identifier.Id);
            
            var scene = identifier.FindScene();
            _propLookup[identifier.Id] = (scene, identifier.AssetId);

            if (AssetManager.Instance.IsSceneLoaded(scene) 
                || !_baseline.TryGetValue(identifier.Id, out var baseline))
            {
                RegisterAdded(
                    scene,
                    identifier.AssetId,
                    current);

                return;
            }

            if (DynamicPropUtility.SameData(current, baseline.data))
            {
                _modified.Remove(identifier.Id);
                return;
            }
            
            if (_modified.TryGetValue(identifier.Id, out var modified))
            {
                identifier.ApplyData(modified);
                return;
            }
            
            RegisterModified(current);
        }

        private void OnUnregistered(PropIdentifier identifier)
        {
            if (!identifier.TryGetDynamicPropData(out var current))
                return;
            
            var scene = identifier.FindScene();
            _propLookup[identifier.Id] = (scene, identifier.AssetId);

            if (AssetManager.Instance.IsSceneLoaded(scene))
            {
                if (_baseline.ContainsKey(identifier.Id))
                {
                    _modified.Remove(identifier.Id);
                    RegisterRemoved(identifier.Id);
                    return;
                }

                RemoveAdded(identifier.Id);
                return;
            }
            
            if (_baseline.TryGetValue(identifier.Id, out var baseline))
            {
                if (DynamicPropUtility.SameData(current, baseline.data))
                {
                    _modified.Remove(identifier.Id);
                    return;
                }
                
                if (_modified.TryGetValue(identifier.Id, out var modified))
                {
                    identifier.ApplyData(modified);
                    return;
                }
                
                RegisterModified(current);
                return;
            }
            
            UpdateAddedData(identifier.Id, current);
        }

        #endregion

        // REGISTRATION UTILS
        
        #region ADDED

        public void RegisterAdded(
            NamedScene scene,
            string assetAddress,
            DynamicPropData prop)
        {
            if (!_added.TryGetValue(scene, out var sceneDict))
            {
                sceneDict = new Dictionary<string, List<DynamicPropData>>();
                _added.Add(scene, sceneDict);
            }

            if (!sceneDict.TryGetValue(assetAddress, out var list))
            {
                list = new List<DynamicPropData>();
                sceneDict.Add(assetAddress, list);
            }

            var copy = DynamicPropUtility.Clone(prop);

            list.Add(copy);

            _addedLookup[prop.id] = new AddedInfo
            {
                scene = scene,
                assetAddress = assetAddress,
                data = copy
            };
        }
        
        public bool RemoveAdded(string id)
        {
            if (!_addedLookup.Remove(id, out var info))
                return false;

            if (!_added.TryGetValue(info.scene, out var sceneDict))
                return true;

            if (!sceneDict.TryGetValue(info.assetAddress, out var list))
                return true;

            list.Remove(info.data);

            if (list.Count == 0)
                sceneDict.Remove(info.assetAddress);

            if (sceneDict.Count == 0)
                _added.Remove(info.scene);

            return true;
        }
        
        private bool TryGetAdded(
            string id,
            out AddedInfo info)
        {
            return _addedLookup.TryGetValue(id, out info);
        }
        
        private void UpdateAddedData(
            string id,
            DynamicPropData data)
        {
            if (!_addedLookup.TryGetValue(id, out var info))
                return;

            info.data = DynamicPropUtility.Clone(data);

            if (!_added.TryGetValue(info.scene, out var sceneDict))
                return;

            if (!sceneDict.TryGetValue(info.assetAddress, out var list))
                return;

            var index = list.FindIndex(x => x.id == id);

            if (index >= 0)
                list[index] = info.data;
        }

        #endregion

        #region MODIFIED & REMOVED

        public void RegisterModified(DynamicPropData prop)
        {
            _modified[prop.id] = DynamicPropUtility.Clone(prop);
            _removed.Remove(prop.id);
        }

        public void RegisterRemoved(string id)
        {
            _modified.Remove(id);
            _removed.Add(id);
        }

        #endregion

        #region INITIALIZATION

        private void OnSceneLoadedListener(NamedScene scene)
            => InitializeBaselineForScene(GameLoader.GetOriginalProfile(scene));

        public void InitializeBaselineForScene(SceneProfile profile)
        {
            foreach (var collection in profile.assetData.collections)
            {
                foreach (var prop in collection.dynamicProps)
                {
                    _baseline[prop.id] = new BaselineInfo
                    {
                        scene = profile.sceneName,
                        assetAddress = collection.assetAddress,
                        data = DynamicPropUtility.Clone(prop)
                    };

                    _removed.Add(prop.id);
                }
            }
        }
        
        private void OnSceneDeloadedListener(NamedScene scene)
            => DeinitializeBaselineForScene(GameLoader.GetOriginalProfile(scene));
        
        public void DeinitializeBaselineForScene(SceneProfile profile)
        {
            foreach (var prop in profile.assetData.collections.SelectMany(collection => collection.dynamicProps))
            {
                _baseline.Remove(prop.id);
            }
        }
        
        #endregion

        #region SAVE & REFRESH

        public void RefreshLoadedScenes()
        {
            foreach (var scene in AssetManager.Instance.GetLoadedScenes())
            {
                RefreshScene(scene);
            }
        }
        
        private void RefreshScene(NamedScene scene)
        {
            foreach (var identifier in AssetManager.Registry.GetByScene(scene))
            {
                if (!identifier.TryGetDynamicPropData(out var current))
                    continue;

                var id = identifier.Id;

                if (_addedLookup.ContainsKey(id))
                {
                    UpdateAddedData(id, current);
                    continue;
                }

                if (_removed.Contains(id))
                    continue;

                if (!_baseline.TryGetValue(id, out var baseline))
                    continue;

                if (DynamicPropUtility.SameData(current, baseline.data))
                {
                    _modified.Remove(id);
                }
                else
                {
                    RegisterModified(current);
                }
            }
        }
        
        public SaveData BuildSaveData()
        {
            RefreshLoadedScenes();

            var save = new SaveData
            {
                timeStamp = DateTimeOffset.UtcNow
            };

            var scenes = new Dictionary<NamedScene, Dictionary<string, SavedProps>>();

            foreach (var pair in _modified)
            {
                if (!_baseline.TryGetValue(pair.Key, out var baseline))
                    continue;

                Add(
                    baseline.scene,
                    baseline.assetAddress,
                    new SavedPropChange
                    {
                        status = PropSceneStatus.Changed,
                        data = DynamicPropUtility.Clone(pair.Value)
                    });
            }

            foreach (var id in _removed)
            {
                if (!_baseline.TryGetValue(id, out var baseline))
                    continue;

                Add(
                    baseline.scene,
                    baseline.assetAddress,
                    new SavedPropChange
                    {
                        status = PropSceneStatus.Removed,
                        data = new DynamicPropData
                        {
                            id = id
                        }
                    });
            }

            foreach (var scene in _added)
            {
                foreach (var asset in scene.Value)
                {
                    foreach (var prop in asset.Value)
                    {
                        Add(
                            scene.Key,
                            asset.Key,
                            new SavedPropChange
                            {
                                status = PropSceneStatus.Added,
                                data = DynamicPropUtility.Clone(prop)
                            });
                    }
                }
            }

            foreach (var scene in scenes)
            {
                save.sceneSaveData.Add(new SceneSaveData
                {
                    scene = scene.Key,
                    collections = scene.Value.Values.ToList()
                });
            }

            return save;

            void Add(
                NamedScene scene,
                string assetAddress,
                SavedPropChange change)
            {
                if (!scenes.TryGetValue(scene, out var collections))
                {
                    collections = new Dictionary<string, SavedProps>();
                    scenes.Add(scene, collections);
                }

                if (!collections.TryGetValue(assetAddress, out var props))
                {
                    props = new SavedProps
                    {
                        assetAddress = assetAddress
                    };

                    collections.Add(assetAddress, props);
                }

                props.changes.Add(change);
            }
        }
        
        public void Initialize(
            SaveData saveData,
            Action<int> declareSubprocessesCount,
            Action<int> declareStepsCallback,
            Action<string> declareStep)
        {
            ClearChanges();

            if (saveData == null)
                return;

            declareSubprocessesCount.Invoke(saveData.sceneSaveData.Count);

            foreach (var sceneData in saveData.sceneSaveData)
            {
                foreach (var collection in sceneData.collections)
                {
                    foreach (var change in collection.changes)
                    {
                        switch (change.status)
                        {
                            case PropSceneStatus.Changed:
                                RegisterModified(change.data);
                                break;

                            case PropSceneStatus.Removed:
                                RegisterRemoved(change.data.id);
                                break;

                            case PropSceneStatus.Added:
                                RegisterAdded(
                                    sceneData.scene,
                                    collection.assetAddress,
                                    change.data);
                                break;
                        }
                    }
                }
            }
        }

        #endregion

        #region UTILS

        public void ApplyToCollection(
            NamedScene scene,
            PropCollection collection)
        {
            collection.dynamicProps = collection.dynamicProps
                .Where(prop => !_removed.Contains(prop.id))
                .Select(prop => DynamicPropUtility.Clone(_modified.GetValueOrDefault(prop.id, prop)))
                .ToList();

            if (_added.TryGetValue(scene, out var sceneDict) &&
                sceneDict.TryGetValue(collection.assetAddress, out var added))
            {
                collection.dynamicProps.AddRange(
                    added.Select(DynamicPropUtility.Clone));
            }
        }
        
        public void ClearChanges()
        {
            _modified.Clear();
            _removed.Clear();
            _added.Clear();
            _addedLookup.Clear();
        }

        #endregion
    }
}