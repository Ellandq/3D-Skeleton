using System.Collections.Generic;
using System.Linq;
using Model.Data.Registry;
using Model.Enum.Named;
using Services.Assets;
using UnityEngine;
using Utils.Enum;

namespace Managers
{
    [RequireComponent(typeof(AssetFactory))]
    public class AssetManager : ManagerBase<AssetManager>
    {
        [Header("Hooks")]
        [SerializeField] private AssetFactory factory;
        private SceneAssetLoader loader;
        private SceneAssetDeloader deloader;
        private AssetRegistry registry;
        
        public static SceneAssetLoader Loader => Instance.loader;
        public static SceneAssetDeloader Deloader => Instance.deloader;
        public static AssetRegistry Registry => Instance.registry;
        public static AssetFactory Factory => Instance.factory;

        protected override void Awake()
        {
            base.Awake();
            
            registry = new AssetRegistry();
            
            loader = new SceneAssetLoader(this, registry, factory);
            deloader = new SceneAssetDeloader(this, registry, factory);
        }

        #region Scene Load State

        private readonly Dictionary<NamedScene, bool> sceneLoadStateDict = new();

        public void RegisterSceneLoad(NamedScene scene)
            => sceneLoadStateDict.Add(scene, true);

        public void RegisterSceneDeload(NamedScene scene)
            => sceneLoadStateDict.Add(scene, false);

        public bool IsSceneLoaded(NamedScene scene) 
            => sceneLoadStateDict.GetValueOrDefault(scene, false);

        public List<NamedScene> GetLoadedScenes()
            => sceneLoadStateDict
                .Where(s => s.Value)
                .Select(s => s.Key)
                .ToList();
        
        public List<NamedScene> GetDeloadedScenes()
            => sceneLoadStateDict
                .Where(s => !s.Value)
                .Select(s => s.Key)
                .ToList();

        #endregion
    }
}