using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Managers;
using UnityEngine;
using UserInterface.Screen;
using Utils.Enum;
using Utils.SO;
using Utils.SO.Scene;

namespace SaveAndLoad
{
    public class GameLoader : MonoBehaviour
    {
        [Header("Scene Profiles")] 
        [SerializeField] private List<SceneProfile> sceneProfiles;
        private Dictionary<NamedScene, SceneProfile> _profiles;

        [Header("Setup Steps")] 
        private LoadQueue _loadQueue;

        [Header("Runtime")] 
        private bool readyToLoad;

        private void Awake()
        {
            _profiles = sceneProfiles.ToDictionary(
                profile => profile.sceneName,
                profile => profile
            );
        }

        public async Task LoadGame(NamedScene sceneName, Action onFinishLoad)
        {
            try
            {
                var profile = _profiles[sceneName];
                var uiManager = UIManager.Instance;
                readyToLoad = !profile.useLoadScreen;

                _loadQueue = new LoadQueue(
                    onFinishLoad,
                    uiManager,
                    SceneManager.Instance
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