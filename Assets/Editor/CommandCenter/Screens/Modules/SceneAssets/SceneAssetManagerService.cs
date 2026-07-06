using System.Collections.Generic;
using System.Linq;
using Components.Props;
using Model.Data.Model;
using SaveAndLoad;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils.Props;

namespace Editor.CommandCenter.Screens.Modules.SceneAssets
{
    public class SceneAssetManagerService
    {
        private readonly ICommandCenterLogger _logger;

        private const string PropsTag = "Props";
        private const string StaticTag = "Static";
        private const string DynamicTag = "Dynamic";
        private const string SceneProfileFolder =
            "Assets/ScriptableObjects/SceneProfiles";
        private string _currentSceneName;
        
        private SceneProfile _currentProfile;

        public SceneAssetManagerService(
            ICommandCenterLogger logger)
        {
            _logger = logger;
        }

        public SceneComparison ScanScene(string sceneName)
        {
            var comparison = new SceneComparison();
            _currentSceneName = sceneName;

            var scene =
                SceneManager.GetSceneByName(sceneName);

            if (!scene.IsValid())
            {
                _logger.LogError(
                    $"Scene {sceneName} not loaded.");

                return comparison;
            }

            _currentProfile =
                LoadSceneProfile(sceneName);

            var saved =
                _currentProfile;

            var propsRoot =
                FindPropsRoot(scene);

            if (!propsRoot)
            {
                _logger.LogWarning(
                    "Props root not found.");

                return comparison;
            }

            var staticRoot =
                FindChildByTag(
                    propsRoot,
                    StaticTag);
            
            var dynamicRoot =
                FindChildByTag(
                    propsRoot,
                    DynamicTag);
            
            if (staticRoot)
            {
                ScanContainer(
                    staticRoot,
                    false,
                    comparison);
            }
            
            if (dynamicRoot)
            {
                ScanContainer(
                    dynamicRoot,
                    true,
                    comparison);
            }

            CompareWithSaved(
                comparison,
                saved);

            return comparison;
        }

        private void ScanContainer(
            GameObject root,
            bool dynamic,
            SceneComparison comparison)
        {
            foreach (Transform child in root.transform)
            {
                ScanObject(
                    child.gameObject,
                    dynamic,
                    comparison);
            }
        }

        private void ScanObject(
            GameObject obj,
            bool dynamic,
            SceneComparison comparison)
        {
            if (!PrefabUtility
                .IsPartOfPrefabInstance(obj))
            {
                _logger.LogWarning(
                    $"{obj.name} is not a prefab.");

                return;
            }

            var prefab =
                PrefabUtility
                .GetCorrespondingObjectFromSource(obj);

            if (!prefab)
                return;

            var address =
                AddressableUtility.GetAddress(prefab);

            if (!string.IsNullOrEmpty(address))
            {
                address = address.Trim();
            }

            if (string.IsNullOrEmpty(address))
            {
                _logger.LogWarning(
                    $"{obj.name} is not Addressable.");
                _logger.LogWarning(
                    $"{obj.name} has no asset path.");
                return;
            }
            
            var type =
                comparison.assetTypes
                    .FirstOrDefault(
                        x => x.address == address);
            
            if (type == null)
            {
                type = new AssetTypeViewModel
                {
                    address = address,
                    displayName =
                        System.IO.Path.GetFileName(address)
                };

                comparison.assetTypes.Add(type);
            }

            var prop =
                new ScenePropViewModel
                {
                    id =
                        PropIdUtility.GetOrCreateId(obj, address).Id,
                    
                    displayName = obj.name,
                    address = address,
                    instance = obj,
                    isDynamic = dynamic,

                    position =
                        obj.transform.position,

                    rotation =
                        obj.transform.rotation,

                    scale =
                        obj.transform.localScale
                };
            
            var saveable =
                obj.GetComponents<MonoBehaviour>()
                    .OfType<ISaveable>()
                    .FirstOrDefault();

            if (saveable != null)
            {
                prop.saveData = saveable.GetSaveData();
            }
            
            if (dynamic)
            {
                var body =
                    obj.GetComponent<Rigidbody>();

                if (body)
                {
                    prop.mass =
                        body.mass;

                    prop.interpolation =
                        body.interpolation;

                    prop.collisionDetectionMode =
                        body.collisionDetectionMode;

                    prop.velocity =
                        body.linearVelocity;

                    prop.angularVelocity =
                        body.angularVelocity;

                    prop.usesGravity =
                        body.useGravity;

                    prop.isKinematic =
                        body.isKinematic;
                }
            }

            type.assets.Add(prop);
        }
        
        private static GameObject FindPropsRoot(
            Scene scene)
        {
            return scene.GetRootGameObjects().FirstOrDefault(root => root.CompareTag(PropsTag));
        }

        private static GameObject FindChildByTag(
            GameObject root,
            string tag)
        {
            return (from Transform child in root.transform where child.CompareTag(tag) select child.gameObject)
                .FirstOrDefault();
        }

        private SceneProfile LoadSceneProfile(
            string sceneName)
        {
            var guid =
                AssetDatabase.FindAssets(
                        $"t:SceneProfile {sceneName}")
                    .FirstOrDefault();

            if (string.IsNullOrEmpty(guid))
            {
                _logger.LogWarning(
                    $"No SceneProfile found for {sceneName}");

                return null;
            }
            
            var path =
                AssetDatabase.GUIDToAssetPath(guid);
            
            return
                AssetDatabase
                    .LoadAssetAtPath<SceneProfile>(
                        path);
        }
        
        private static List<SavedPropData> ExtractSavedProps(
            SceneProfile profile)
        {
            var result =
                new List<SavedPropData>();


            if (!profile ||
                !profile.assetData)
                return result;

            foreach (var collection in
                     profile.assetData.collections)
            {
                result.AddRange(collection.props.Select(prop => new SavedPropData
                {
                    address = collection.assetAddress,
                    id = prop.id,
                    saveData = prop.saveData,

                    position = prop.position,
                    rotation = prop.rotation,
                    scale = prop.scale
                }));


                result.AddRange(collection.dynamicProps.Select(prop => new SavedPropData
                {
                    address = collection.assetAddress,
                    id = prop.id,
                    saveData = prop.saveData,

                    position = prop.position,
                    rotation = prop.rotation,
                    scale = prop.scale,

                    mass = prop.mass,
                    interpolation = prop.interpolation,
                    collisionDetectionMode = prop.collisionDetectionMode,

                    velocity = prop.velocity,
                    angularVelocity = prop.angularVelocity,
                    usesGravity = prop.usesGravity,
                    isKinematic = prop.isKinematic
                }));
            }
            
            return result;
        }
        
        private static void CompareWithSaved(
            SceneComparison comparison,
            SceneProfile profile)
        {
            var saved =
                ExtractSavedProps(profile);

            foreach (var type in comparison.assetTypes)
            {
                foreach (var asset in type.assets)
                {
                    foreach (var s in saved.Where(x => x.address == asset.address))
                    {
                        Debug.Log($"Saved: {s.id}");
                    }

                    Debug.Log($"Scene: {asset.id}");
                    
                    var match =
                        saved.FirstOrDefault(
                            x =>
                                x.address == asset.address &&
                                x.id == asset.id);

                    if (match == null)
                    {
                        Debug.Log($"Missing saved prop: {asset.address} | {asset.id}");
                        asset.isSaved = false;
                        continue;
                    }
                    
                    asset.isSaved = true;
                    
                    asset.isModified =
                        HasChanged(
                            asset,
                            match);
                }
                
                type.isSaved =
                    type.assets.Any(
                        x => x.isSaved);
            }
        }
        
        private static bool HasChanged(
            ScenePropViewModel scene,
            SavedPropData saved)
        {
            if (scene.position != saved.position)
                return true;


            if (scene.rotation != saved.rotation)
                return true;


            if (scene.scale != saved.scale)
                return true;
            
            if (scene.saveData != saved.saveData)
                return true;

            if (!scene.isDynamic) return false;
            if (scene.velocity != saved.velocity)
                return true;


            if (scene.angularVelocity != saved.angularVelocity)
                return true;


            if (!Mathf.Approximately(scene.mass, saved.mass))
                return true;

            if (scene.interpolation != saved.interpolation)
                return true;

            if (scene.collisionDetectionMode != saved.collisionDetectionMode)
                return true;

            if (scene.velocity != saved.velocity)
                return true;

            if (scene.angularVelocity != saved.angularVelocity)
                return true;

            if (scene.usesGravity != saved.usesGravity)
                return true;

            return scene.isKinematic != saved.isKinematic;
        }
        
        public void SpawnMissing(
            SceneComparison comparison)
        {
            if (!_currentProfile ||
                !_currentProfile.assetData)
            {
                _logger.LogError(
                    "No saved asset data.");

                return;
            }

            var propsRoot =
                GameObject.FindGameObjectWithTag(
                    PropsTag);
            
            if (!propsRoot)
            {
                _logger.LogError(
                    "Props root not found.");

                return;
            }

            var staticRoot =
                FindChildByTag(
                    propsRoot,
                    StaticTag);

            var dynamicRoot =
                FindChildByTag(
                    propsRoot,
                    DynamicTag);

            foreach(var collection in
                    _currentProfile.assetData.collections)
            {
                foreach (var prop in collection.props.Where(prop => !IsAlreadySpawned(
                             collection.assetAddress,
                             prop.id,
                             comparison)))
                {
                    SpawnStatic(
                        collection.assetAddress,
                        prop,
                        staticRoot);
                }

                foreach (var prop in collection.dynamicProps.Where(prop => !IsAlreadySpawned(
                             collection.assetAddress,
                             prop.id,
                             comparison)))
                {
                    SpawnDynamic(
                        collection.assetAddress,
                        prop,
                        dynamicRoot);
                }
            }
            
            _logger.Log(
                "Missing assets spawned.");
            
        }
        
        private static bool IsAlreadySpawned(
            string address,
            string id,
            SceneComparison comparison)
        {
            return comparison.assetTypes
                .SelectMany(x => x.assets)
                .Any(x =>
                    x.address == address &&
                    x.id == id);
        }
        
        private static void SpawnStatic(
            string address,
            PropData data,
            GameObject parent)
        {
            var prefab =
                AddressableLoader
                    .LoadPrefab(address);

            if (!prefab)
            {
                Debug.LogError(
                    $"Could not load prefab at address: {address}");

                return;
            }
            
            var instance =
                PrefabUtility.InstantiatePrefab(
                    prefab,
                    parent.scene) as GameObject;
            
            if (!instance)
                return;
            
            instance.transform.SetParent(
                parent.transform);
            
            instance.name =
                prefab.name;

            var identifier =
                instance.GetComponent<PropIdentifier>();

            if (!identifier)
            {
                identifier =
                    instance.AddComponent<PropIdentifier>();
            }

            identifier.Initialize(data.id, address);
            
            ApplyTransform(
                instance,
                data.position,
                data.rotation,
                data.scale);
        }
        
        private static void SpawnDynamic(
            string address,
            DynamicPropData data,
            GameObject parent)
        {
            var prefab =
                AddressableLoader
                    .LoadPrefab(address);

            if (!prefab)
            {
                Debug.LogError(
                    $"Could not load prefab at address: {address}");

                return;
            }
            
            var instance =
                PrefabUtility.InstantiatePrefab(
                    prefab,
                    parent.scene) as GameObject;
            
            if (!instance)
                return;
            
            instance.transform.SetParent(
                parent.transform);

            instance.name =
                prefab.name;
            
            var identifier =
                instance.GetComponent<PropIdentifier>();

            if (!identifier)
            {
                identifier =
                    instance.AddComponent<PropIdentifier>();
            }

            identifier.Initialize(data.id, address);

            ApplyTransform(
                instance,
                data.position,
                data.rotation,
                data.scale);

            var body =
                instance.GetComponentInChildren<Rigidbody>();

            if (!body)
                return;

            body.mass =
                data.mass;

            body.interpolation =
                data.interpolation;

            body.collisionDetectionMode =
                data.collisionDetectionMode;

            body.linearVelocity =
                data.velocity;

            body.angularVelocity =
                data.angularVelocity;

            body.useGravity =
                data.usesGravity;

            body.isKinematic =
                data.isKinematic;
        }

        private static void ApplyTransform(
            GameObject obj,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale)
        {
            obj.transform.position =
                position;

            obj.transform.rotation =
                rotation;

            obj.transform.localScale =
                scale;
        }

        public void Save(
            SceneComparison comparison)
        {
            if (!_currentProfile)
            {
                _logger.LogError(
                    "No SceneProfile loaded.");

                return;
            }

            if (!_currentProfile.assetData)
            {
                var data =
                    ScriptableObject.CreateInstance<SceneAssetData>();
                
                var path =
                    AssetDatabase.GetAssetPath(
                        _currentProfile);
                
                var folder =
                    System.IO.Path.GetDirectoryName(path);
                
                var dataPath =
                    $"{folder}/{_currentProfile.name}_AssetData.asset";
                
                AssetDatabase.CreateAsset(
                    data,
                    dataPath);
                
                _currentProfile.assetData = data;
                
                EditorUtility.SetDirty(
                    _currentProfile);
            }

            var assetData =
                _currentProfile.assetData;


            assetData.collections.Clear();

            foreach(var type in comparison.assetTypes)
            {
                var collection =
                    new PropCollection
                    {
                        assetAddress = type.address,

                        props =
                            new List<PropData>(),

                        dynamicProps =
                            new List<DynamicPropData>()
                    };

                foreach(var prop in type.assets)
                {
                    if(prop.isDynamic)
                    {
                        collection.dynamicProps.Add(
                            CreateDynamicProp(prop));
                    }
                    else
                    {
                        collection.props.Add(
                            CreateProp(prop));
                    }
                }

                assetData.collections.Add(
                    collection);
            }
            
            EditorUtility.SetDirty(
                assetData);

            AssetDatabase.SaveAssets();
            
            _logger.Log(
                "Scene assets saved.");
        }
        
        private static PropData CreateProp(
            ScenePropViewModel source)
        {
            return new PropData
            {
                id = source.id,
                saveData = source.saveData,
                position = source.position,
                rotation = source.rotation,
                scale = source.scale
            };
        }

        private static DynamicPropData CreateDynamicProp(
            ScenePropViewModel source)
        {
            return new DynamicPropData
            {
                id = source.id,
                saveData = source.saveData,

                position = source.position,
                rotation = source.rotation,
                scale = source.scale,

                mass = source.mass,
                interpolation = source.interpolation,
                collisionDetectionMode = source.collisionDetectionMode,

                velocity = source.velocity,
                angularVelocity = source.angularVelocity,
                usesGravity = source.usesGravity,
                isKinematic = source.isKinematic
            };
        }

        public void DestroySceneProps()
        {
            var propsRoot =
                GameObject.FindGameObjectWithTag(
                    PropsTag);
            
            if(!propsRoot)
            {
                _logger.LogWarning(
                    "Props root not found.");

                return;
            }
            
            var staticRoot =
                FindChildByTag(
                    propsRoot,
                    StaticTag);
            
            var dynamicRoot =
                FindChildByTag(
                    propsRoot,
                    DynamicTag);

            DestroyChildren(staticRoot);

            DestroyChildren(dynamicRoot);
            
            _logger.Log(
                "Scene props destroyed.");
        }
        
        private static void DestroyChildren(
            GameObject root)
        {
            if(!root)
                return;

            var children = (from Transform child in root.transform select child.gameObject).ToList();

            foreach(var child in children)
            {
                Undo.DestroyObjectImmediate(
                    child);
            }
        }
    }
}