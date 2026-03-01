using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Managers;
using UnityEngine;
using UserInterface.Screen;
using Utils.Enum;
using Utils.SO;

namespace SaveAndLoad
{
    public class GameLoader : MonoBehaviour
    {
        [Header("Scene Profiles")] 
        [SerializeField] private List<SceneProfile> sceneProfiles;
        private Dictionary<NamedScene, SceneProfile> _profiles;

        [Header("Setup Steps")] 
        private LoadQueue _loadQueue;

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

                _loadQueue = new LoadQueue(
                    onFinishLoad,
                    uiManager,
                    SceneManager.Instance
                );

                if (profile.useLoadScreen)
                {
                    var loadingScreen = (LoadingScreen)uiManager.GetComponent(NamedScreen.Loading);
                    uiManager.ActivateComponent(NamedScreen.Loading);
                    loadingScreen.Bind(_loadQueue);
                }

                await _loadQueue.StartLoad(profile);
            }
            catch (KeyNotFoundException)
            {
                Debug.LogError($"Could not find scene profile for: {sceneName}");
            }
        }
    }
}