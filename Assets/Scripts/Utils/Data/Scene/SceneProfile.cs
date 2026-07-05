using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UserInterface.HUD;
using UserInterface.Overlay;
using UserInterface.Screen;
using UserInterface.Windows;
using Utils.Data.Save;
using Utils.Enum;

namespace Utils.Data.Scene
{
    [CreateAssetMenu(menuName = "Scenes/Scene Profile")]
    public class SceneProfile : ScriptableObject
    {
        [Header("Scene Settings")]
        public NamedScene sceneName;
        public List<NamedScene> subScenes = new();
        private Dictionary<NamedScene, SceneProfile> _subSceneProfiles = new();
        public bool useLoadScreen;
        public bool saveChanges;
        
        
        
        [Header("UI Assets")]
        [SerializeField] private List<NamedHUD> hudKeys = new();
        public List<NamedHUD> HudKeys =>
            hudKeys.Union(_subSceneProfiles.Values.SelectMany(p => p.hudKeys)).ToList();

        [SerializeField] private List<NamedOverlay> overlayKeys = new();
        public List<NamedOverlay> OverlayKeys =>
            overlayKeys.Union(_subSceneProfiles.Values.SelectMany(p => p.overlayKeys)).ToList();

        [SerializeField] private List<NamedScreen> screenKeys = new();
        public List<NamedScreen> ScreenKeys =>
            screenKeys.Union(_subSceneProfiles.Values.SelectMany(p => p.screenKeys)).ToList();

        [SerializeField] private List<NamedWindow> windowKeys = new();
        public List<NamedWindow> WindowKeys =>
            windowKeys.Union(_subSceneProfiles.Values.SelectMany(p => p.windowKeys)).ToList();

        
        
        [Header("Props")] 
        public SceneAssetData assetData;
        public SceneSaveData AssetChanges { get; private set; }


        public void AssignSubSceneProfile(NamedScene scene, SceneProfile profile)
        {
            _subSceneProfiles.Add(scene, profile);
        }
        
        public void ApplySaveData(SaveData saveData)
        {
            var saveLookup = saveData.sceneSaveData.ToDictionary(s => s.scene);

            if (saveLookup.TryGetValue(sceneName, out var ownSave))
                SetAssetChanges(ownSave);

            foreach (var (scene, profile) in _subSceneProfiles)
            {
                if (saveLookup.TryGetValue(scene, out var sceneSave))
                    profile.SetAssetChanges(sceneSave);
            }
        }

        public void SetAssetChanges(SceneSaveData assetChanges)
        {
            AssetChanges = assetChanges;
        }

        public Dictionary<string, Dictionary<string, DynamicPropData>> GetCollectionDictionary()
            => assetData.GetCollectionDictionary();
        
        public Dictionary<NamedScene, Dictionary<string, Dictionary<string, DynamicPropData>>> GetAllPropData()
        {
            var result = new Dictionary<NamedScene, Dictionary<string, Dictionary<string, DynamicPropData>>>
            {
                [sceneName] = GetCollectionDictionary()
            };

            foreach (var profile in _subSceneProfiles.Values)
            {
                result[profile.sceneName] = profile.GetCollectionDictionary();
            }

            return result;
        }
        
        public List<PropCollection> GetCollectionsWithChangesApplied()
        {
            var collections = assetData.collections
                .Select(c => c.Clone())
                .ToList();

            if (AssetChanges == null)
                return collections;

            var collectionLookup = collections.ToDictionary(c => c.assetAddress);

            foreach (var savedCollection in AssetChanges.collections)
            {
                if (!collectionLookup.TryGetValue(savedCollection.assetAddress, out var collection))
                {
                    collection = new PropCollection
                    {
                        assetAddress = savedCollection.assetAddress
                    };

                    collections.Add(collection);
                    collectionLookup.Add(collection.assetAddress, collection);
                }

                var propLookup = collection.dynamicProps.ToDictionary(p => p.id);

                foreach (var (data, status) in savedCollection.dynamicProps)
                {
                    switch (status)
                    {
                        case PropSceneStatus.Added:
                            if (!propLookup.ContainsKey(data.id))
                            {
                                var clone = (DynamicPropData)data.Clone();
                                collection.dynamicProps.Add(clone);
                                propLookup.Add(clone.id, clone);
                            }
                            break;

                        case PropSceneStatus.Removed:
                            if (propLookup.TryGetValue(data.id, out var removed))
                            {
                                collection.dynamicProps.Remove(removed);
                                propLookup.Remove(data.id);
                            }
                            break;

                        case PropSceneStatus.Changed:
                            if (propLookup.TryGetValue(data.id, out var existing))
                            {
                                collection.dynamicProps.Remove(existing);
                            }

                            var changed = (DynamicPropData)data.Clone();
                            collection.dynamicProps.Add(changed);
                            propLookup[data.id] = changed;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            return collections;
        }
        
        public IEnumerable<SceneProfile> SubProfiles()
        {
            return _subSceneProfiles.Values;
        }
    }
}