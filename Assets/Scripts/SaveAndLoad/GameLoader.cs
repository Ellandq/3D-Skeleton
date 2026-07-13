using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Managers;
using Model.Data.Save;
using Model.Data.Scene;
using Model.SO.Scene;
using UnityEngine;
using UserInterface.Screen;
using Utils.Enum;

namespace SaveAndLoad
{
    public class GameLoader : MonoBehaviour
    {
        private static GameLoader _instance;
        
        [Header("Scene Profiles")] 
        [SerializeField] private List<SceneProfile> sceneProfiles;
        private Dictionary<NamedScene, SceneProfile> _profiles;

        [Header("Setup Steps")] 
        private LoadQueue _loadQueue;

        [Header("Runtime")] 
        private bool readyToLoad;

        private void Awake()
        {
            _instance = this;
            
            _profiles = sceneProfiles.ToDictionary(
                profile => profile.sceneName,
                profile => profile
            );
        }

        public async UniTask LoadGame(
            NamedScene sceneName,
            Action onFinishLoad)
        {
            try
            {
                var profile = Construct(sceneName);

                readyToLoad = !profile.useLoadScreen;

                _loadQueue = new LoadQueue(
                    onFinishLoad,
                    UIManager.Instance,
                    SaveManager.Instance,
                    AssetManager.Deloader,
                    SceneManager.Instance,
                    AssetManager.Loader
                );

                if (profile.useLoadScreen) // && SceneManager.CanLoadWithoutLoadingScreen(profile.GetAllScenes())
                {
                    var loadingScreen =
                        UIManager.GetUIComponent<NamedScreen, LoadingScreen>(
                            NamedScreen.Loading);

                    UIManager.ActivateComponent(
                        NamedScreen.Loading,
                        false,
                        () => readyToLoad = true);

                    loadingScreen.Bind(_loadQueue);
                }

                while (!readyToLoad)
                    await Task.Yield();

                await _loadQueue.StartLoad(profile);
            }
            catch (KeyNotFoundException)
            {
                Debug.LogError($"Could not find scene profile for: {sceneName}");
            }
        }


        private RuntimeSceneProfile Construct(NamedScene scene)
        {
            var profile = RuntimeSceneProfile.BuildRuntimeProfile(
                _profiles[scene],
                SaveManager.Registry);

            profile.InitializeSubSceneProfiles(
                _profiles[scene]
                    .subScenes
                    .Select(Construct));

            return profile;
        }

        public static SceneProfile GetOriginalProfile(NamedScene scene)
            => _instance._profiles[scene];
    }
}