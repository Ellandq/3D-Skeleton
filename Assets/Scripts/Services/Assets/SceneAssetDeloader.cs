using System;
using System.Collections.Generic;
using System.Linq;
using Components.Props;
using Cysharp.Threading.Tasks;
using Managers;
using Model.Data.Registry;
using Model.Data.Scene;
using UnityEngine;
using Utils.Contract;

namespace Services.Assets
{
    public class SceneAssetDeloader : SceneAssetManager, IAsyncInitializable
    {
        public SceneAssetDeloader(AssetManager assetManager, AssetRegistry registry, AssetFactory factory)
            : base(assetManager, registry, factory) { }

        public string ProcessName => "Asset Deloader";

        public async UniTask InitializeForScene(
            RuntimeSceneProfile sceneProfile,
            Action<int> declareSubprocessesCount,
            Action<int> declareStepsCallBack,
            Action<string> declareStep)
        {
            var loaded = assetManager.GetLoadedScenes();
            
            var toLoad = sceneProfile.SubScenes
                .Except(loaded)
                .ToList();
            
            var loadedToDeload = loaded
                .Except(toLoad)
                .ToList();

            if (loadedToDeload.Count == 0)
            {
                declareSubprocessesCount.Invoke(1);
                declareStepsCallBack.Invoke(1);
                declareStep.Invoke("Nothing to unload - Skipping...");
                return;
            }

            foreach (var scene in loadedToDeload)
            {
                assetManager.RegisterSceneDeload(scene);
            }
                
            
            if (Time.timeScale != 0f)
            {
                await UniTask.WaitForFixedUpdate();
            }

            if (toLoad.Count == 0)
            {
                declareSubprocessesCount.Invoke(loadedToDeload.Count);
                
                foreach (var scene in loadedToDeload)
                {
                    await DeloadSceneAssets(registry.GetCollectionByScene(scene), declareStepsCallBack, declareStep);
                }
                
                return;
            }
            
            declareSubprocessesCount.Invoke(1);

            var deloadSceneAssetRefs = loadedToDeload
                .SelectMany(scene => registry.GetCollectionByScene(scene))
                .GroupBy(x => x.Key)
                .ToDictionary(
                    g => g.Key,
                    g => g
                        .SelectMany(x => x.Value)
                        
                        .Select(k => k.Value)
                        .ToList()
                );
            
            var loadSceneAssets = sceneProfile
                .GetAllProfiles()
                .ToDictionary(
                    kv => kv.Key,
                    kv => kv.Value.assetData.collections.ToList()
                );

            var availablePool = deloadSceneAssetRefs
                .ToDictionary(
                    kv => kv.Key,
                    kv => new Queue<PropIdentifier>(kv.Value)
                );
            
            var assetsToManage = availablePool.Count;
            var completed = 0;
            
            declareStepsCallBack.Invoke(assetsToManage);
            
            
            foreach (var (key, collections) in loadSceneAssets)
            {
                foreach (var collection in collections)
                {
                    var address = collection.assetAddress;

                    var requiredCount =
                        collection.props.Count +
                        collection.dynamicProps.Count;

                    if (!availablePool.TryGetValue(address, out var queue))
                        continue;

                    for (var i = 0; i < requiredCount; i++)
                    {
                        if (queue.Count == 0)
                            break;

                        var reusable = queue.Dequeue();

                        factory.PushToPool(reusable);
                        registry.Unregister(reusable.Id);

                        completed++;
                        declareStep.Invoke($"Preserving asset - ({completed}/{assetsToManage})");
                    }
                }
            }
            
            foreach (var queue in availablePool.Values)
            {
                while (queue.Count > 0)
                {
                    var identifier = queue.Dequeue();

                    registry.Unregister(identifier.Id);

                    factory.Release(identifier.gameObject);
                    
                    completed++;
                    declareStep.Invoke($"Deloading asset - ({completed}/{assetsToManage})");
                }
            }
        }

        private async UniTask DeloadSceneAssets(
            IReadOnlyDictionary<string, Dictionary<string, PropIdentifier>> sceneProps,
            Action<int> declareStepsCallBack,
            Action<string> declareStep)
        {
            if (sceneProps == null || sceneProps.Count == 0)
            {
                declareStepsCallBack.Invoke(1);
                declareStep.Invoke("No assets found to deload.");
                return;
            }

            var total = sceneProps.Sum(kvp => kvp.Value.Count);
            declareStepsCallBack.Invoke(total);

            var step = 0;

            var assets = sceneProps.ToList();

            foreach (var (assetAddress, propDict) in assets)
            {
                if (propDict == null || propDict.Count == 0)
                    continue;

                declareStep.Invoke($"Deloading asset: {assetAddress}");

                var props = propDict.Values.ToList();

                foreach (var prop in props)
                {
                    step++;
                    declareStep.Invoke($"Recycling {prop.Id} ({step}/{total})");

                    if (!prop)
                        continue;

                    registry.Unregister(prop.Id);

                    factory.PushToPool(prop);

                    if (step % 25 == 0)
                        await UniTask.Yield();
                }
            }

            declareStep.Invoke("Deload complete.");
        }
    }
}