using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Managers;
using Model.Data.Registry;
using Model.Data.Scene;
using UnityEngine;
using Utils.Contract;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Services.Assets
{
    public class SceneAssetLoader : SceneAssetManager, IAsyncInitializable
    {
        public SceneAssetLoader(AssetManager assetManager, AssetRegistry registry, AssetFactory factory)
            : base(assetManager, registry, factory) { }
        
        public string ProcessName => "Asset Loader";

        public async UniTask InitializeForScene(
            RuntimeSceneProfile sceneProfile, 
            Action<int> declareSubprocessesCount, 
            Action<int> declareStepsCallBack,
            Action<string> declareStep)
        {
            if (Time.timeScale != 0f)
            {
                await UniTask.WaitForFixedUpdate();
            }
            
            var loadSceneAssets = sceneProfile
                .GetAllProfiles()
                .ToDictionary(
                    kv => kv.Key,
                    kv => kv.Value.assetData.collections.ToList()
                );

            declareSubprocessesCount?.Invoke(loadSceneAssets.Count);
            
            foreach (var (scene, value) in loadSceneAssets)
            {
                try
                {
                    var (dynamicRoot, staticRoot) = GetPropTransforms(scene.ToString());
                    var stepCount = value.Sum(p => p.dynamicProps.Count + p.props.Count);
                    var completed = 0;
                    declareStepsCallBack.Invoke(stepCount);
                    foreach (var collection in value)
                    {
                        var address = collection.assetAddress;

                        foreach (var prop in collection.dynamicProps)
                        {
                            var dy = await factory.InstantiateAsync(address, prop, dynamicRoot);
                            
                            registry.Register(dy);
                            
                            var rb = dy.Rigidbody;
                            if (rb)
                            {
                                rb.linearVelocity = prop.velocity;
                                rb.angularVelocity = prop.angularVelocity;
                                rb.collisionDetectionMode = prop.collisionDetectionMode;
                                rb.interpolation = prop.interpolation;
                                rb.useGravity = prop.usesGravity;
                                rb.isKinematic = prop.isKinematic;
                            }
                            var sv = dy.Savable;
                            sv?.LoadSaveData(prop.saveData);
                            completed++;
                            declareStep.Invoke($"Loading assets for scene: {scene} - ({completed}/{stepCount})");
                        }

                        foreach (var prop in collection.props)
                        {
                            var st = await factory.InstantiateAsync(address, prop, staticRoot);
                            completed++;
                            declareStep.Invoke($"Loading assets for scene: {scene} - ({completed}/{stepCount})");
                        }
                    }
                    assetManager.RegisterSceneLoad(scene);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                    declareStepsCallBack.Invoke(1);
                    declareStep.Invoke("Failed to fetch root - Skipping");
                }
            }
        }
        
        private static (Transform dynamicTransform, Transform staticTransform) GetPropTransforms(string sceneName)
        {
            var scene = SceneManager.GetSceneByName(sceneName);

            if (!scene.isLoaded)
                throw new Exception($"Scene {sceneName} is not loaded.");

            foreach (var rootObject in scene.GetRootGameObjects())
            {
                if (!rootObject.CompareTag("Props"))
                    continue;

                var props = rootObject.transform;

                Transform dynamic = null;
                Transform statics = null;

                foreach (Transform child in props)
                {
                    if (child.CompareTag("Dynamic"))
                        dynamic = child;

                    if (child.CompareTag("Static"))
                        statics = child;
                }

                if (!dynamic || !statics)
                    throw new Exception("Missing Dynamic or Static prop containers.");

                return (dynamic, statics);
            }

            throw new Exception($"No Props object found in scene {sceneName}");
        }
    }
}