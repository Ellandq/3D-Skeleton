using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Managers;
using Model.Data.Model;
using Model.Data.Save;
using Model.Enum.Named;
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
            
            sceneProfiles.ForEach(
                p => p.subScenes.ForEach(
                    s => p.AssignSubSceneProfile(s, _profiles[s])));
        }

        public async UniTask LoadGame(NamedScene sceneName, Action onFinishLoad, [CanBeNull] SaveData saveData)
        {
            try
            {
                var profile = _profiles[sceneName];
                var uiManager = UIManager.Instance;
                readyToLoad = !profile.useLoadScreen;

                _loadQueue = new LoadQueue(
                    onFinishLoad,
                    uiManager,
                    AssetManager.Deloader,
                    SceneManager.Instance,
                    AssetManager.Loader
                );

                if (profile.useLoadScreen)
                {
                    var loadingScreen = UIManager.GetUIComponent<NamedScreen, LoadingScreen>(NamedScreen.Loading);
                    UIManager.ActivateComponent(NamedScreen.Loading, false, () => readyToLoad = true);
                    loadingScreen.Bind(_loadQueue);
                }

                while (!readyToLoad) await Task.Yield();

                await _loadQueue.StartLoad(profile);
            }
            catch (KeyNotFoundException)
            {
                Debug.LogError($"Could not find scene profile for: {sceneName}");
            }
        }
    }
}