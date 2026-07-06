using System;
using System.Collections.Generic;
using System.Linq;
using Components.Props;
using Cysharp.Threading.Tasks;
using Managers;
using Model.Data.Model;
using Model.Data.Registry;
using Utils.Contract;

namespace Services.Assets
{
    public class SceneAssetDeloader : SceneAssetManager, IAsyncInitializable
    {
        public SceneAssetDeloader(AssetManager assetManager, AssetRegistry registry, AssetFactory factory)
            : base(assetManager, registry, factory) { }

        public string ProcessName => "Asset Deloader";

        public async UniTask InitializeForScene(
            SceneProfile sceneProfile,
            Action<int> declareSubprocessesCount,
            Action<int> declareStepsCallBack,
            Action<string> declareStep)
        {
            var loaded = assetManager.GetLoadedScenes();
            
            var toLoad = sceneProfile.GetAllScenes()
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

            if (toLoad.Count == 0)
            {
                declareSubprocessesCount.Invoke(loadedToDeload.Count);
                
                foreach (var scene in loadedToDeload)
                {
                    await DeloadSceneAssets(registry.GetCollectionByScene(scene), declareStepsCallBack, declareStep);
                }
                
                return;
            }

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

            var availableCounts = deloadSceneAssetRefs
                .ToDictionary(
                    kv => kv.Key,
                    kv => new Queue<PropIdentifier>(kv.Value)
                );
            
            foreach (var kvp in loadSceneAssets)
            {
                var collections = kvp.Value;

                foreach (var collection in collections)
                {
                    var address = collection.assetAddress;

                    var requiredCount =
                        collection.props.Count +
                        collection.dynamicProps.Count;

                    if (!availableCounts.TryGetValue(address, out var queue))
                        continue;

                    for (var i = 0; i < requiredCount; i++)
                    {
                        if (queue.Count == 0)
                            break;

                        var reusable = queue.Dequeue();

                        factory.PushToPool(reusable);
                        registry.Unregister(reusable.Id);
                    }
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

            foreach (var (assetAddress, propDict) in sceneProps)
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

                    factory.PushToPool(prop);

                    registry.Unregister(prop.Id);

                    if (step % 25 == 0)
                        await UniTask.Yield();
                }
            }

            declareStep.Invoke("Deload complete.");
        }
    }
}