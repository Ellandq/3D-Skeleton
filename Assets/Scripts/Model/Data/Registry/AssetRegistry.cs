using System;
using System.Collections.Generic;
using Components.Props;
using JetBrains.Annotations;
using Model.Enum.Named;
using Utils.Enum;

namespace Model.Data.Registry
{
    public class AssetRegistry
    {
        // Primary registry
        private readonly Dictionary<string, PropIdentifier> _byId = new();
        private readonly Dictionary<string, List<PropIdentifier>> _byAsset = new();

        // Scene-based indexes
        private readonly Dictionary<NamedScene, Dictionary<string, Dictionary<string, PropIdentifier>>> _fromScene = new();

        // Events
        public event Action<PropIdentifier> Registered;
        public event Action<PropIdentifier> Unregistered;

        // -------------------------
        // REGISTER
        // -------------------------
        public void Register(PropIdentifier identifier)
        {
            if (!identifier)
                throw new ArgumentNullException(nameof(identifier));

            if (!_byId.TryAdd(identifier.Id, identifier))
                throw new InvalidOperationException(
                    $"A prop with id '{identifier.Id}' is already registered.");

            if (!_byAsset.TryGetValue(identifier.AssetId, out var assetList))
            {
                assetList = new List<PropIdentifier>();
                _byAsset.Add(identifier.AssetId, assetList);
            }
            assetList.Add(identifier);

            var scene = GetScene(identifier);

            if (!_fromScene.TryGetValue(scene, out var sceneDict))
            {
                sceneDict = new Dictionary<string, Dictionary<string, PropIdentifier>>();
                _fromScene.Add(scene, sceneDict);
            }

            if (!sceneDict.TryGetValue(identifier.AssetId, out var assetDict))
            {
                assetDict = new Dictionary<string, PropIdentifier>();
                sceneDict.Add(identifier.AssetId, assetDict);
            }

            assetDict[identifier.Id] = identifier;

            Registered?.Invoke(identifier);
        }

        // -------------------------
        // UNREGISTER
        // -------------------------
        public void Unregister(string id)
        {
            if (!_byId.Remove(id, out var identifier))
                return;

            if (_byAsset.TryGetValue(identifier.AssetId, out var assetList))
            {
                assetList.Remove(identifier);
                if (assetList.Count == 0)
                    _byAsset.Remove(identifier.AssetId);
            }

            var scene = GetScene(identifier);

            if (_fromScene.TryGetValue(scene, out var sceneDict))
            {
                if (sceneDict.TryGetValue(identifier.AssetId, out var assetDict))
                {
                    assetDict.Remove(identifier.Id);

                    if (assetDict.Count == 0)
                        sceneDict.Remove(identifier.AssetId);
                }

                if (sceneDict.Count == 0)
                    _fromScene.Remove(scene);
            }

            Unregistered?.Invoke(identifier);
        }

        public bool Unregister(PropIdentifier identifier)
        {
            if (!identifier)
                return false;

            if (!_byId.ContainsKey(identifier.Id))
                return false;

            Unregister(identifier.Id);
            return true;
        }

        // -------------------------
        // SCENE HELPERS
        // -------------------------
        private static NamedScene GetScene(PropIdentifier identifier)
        {
            var sceneName = identifier.gameObject.scene.name;
            return System.Enum.Parse<NamedScene>(sceneName);
        }

        public bool TryGetScene(PropIdentifier identifier, out NamedScene scene)
        {
            scene = default;

            if (!identifier || !identifier.gameObject)
                return false;

            scene = GetScene(identifier);
            return true;
        }

        // -------------------------
        // SCENE QUERIES
        // -------------------------
        public IReadOnlyDictionary<string, Dictionary<string, PropIdentifier>> GetCollectionByScene(NamedScene scene)
        {
            return _fromScene.TryGetValue(scene, out var dict)
                ? dict
                : new Dictionary<string, Dictionary<string, PropIdentifier>>();
        }

        public IReadOnlyDictionary<string, PropIdentifier> GetByScene(NamedScene scene, string assetId)
        {
            if (_fromScene.TryGetValue(scene, out var sceneDict) &&
                sceneDict.TryGetValue(assetId, out var assetDict))
            {
                return assetDict;
            }

            return new Dictionary<string, PropIdentifier>();
        }

        public bool TryGetFromScene(NamedScene scene, string assetId, string id, out PropIdentifier identifier)
        {
            identifier = null;

            return _fromScene.TryGetValue(scene, out var sceneDict) &&
                   sceneDict.TryGetValue(assetId, out var assetDict) &&
                   assetDict.TryGetValue(id, out identifier);
        }

        // -------------------------
        // BASIC QUERIES
        // -------------------------
        public bool IsRegistered(PropIdentifier identifier)
        {
            return identifier &&
                   _byId.TryGetValue(identifier.Id, out var reg) &&
                   reg == identifier;
        }

        [CanBeNull]
        public PropIdentifier Get(string id)
        {
            _byId.TryGetValue(id, out var identifier);
            return identifier;
        }

        public bool TryGet(string id, out PropIdentifier identifier)
        {
            return _byId.TryGetValue(id, out identifier);
        }

        public IReadOnlyList<PropIdentifier> GetByAsset(string assetId)
        {
            return _byAsset.TryGetValue(assetId, out var list)
                ? list
                : Array.Empty<PropIdentifier>();
        }

        public IEnumerable<PropIdentifier> GetAll()
        {
            return _byId.Values;
        }

        public bool Contains(string id)
        {
            return _byId.ContainsKey(id);
        }

        // -------------------------
        // CLEAR
        // -------------------------
        public void Clear()
        {
            _byId.Clear();
            _byAsset.Clear();
            _fromScene.Clear();
        }
    }
}