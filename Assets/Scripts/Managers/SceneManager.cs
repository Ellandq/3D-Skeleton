using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using Utils.Collections;
using Utils.Contract;
using Utils.Enum;
using Utils.SO;

namespace Managers
{
    public class SceneManager : ManagerBase<SceneManager>, IAsyncInitializable
    {
        private readonly HashSet<NamedScene> _loadedScenes = new();
        private bool _isLoading;

        public bool IsLoading => _isLoading;

        public IReadOnlyCollection<NamedScene> LoadedScenes => _loadedScenes;
        
        public event Action<NamedScene> OnSceneLoaded;
        public event Action<NamedScene> OnSceneUnloaded;

        public async Task LoadSceneAdditiveAsync(NamedScene sceneName, bool setActive = true)
        {
            if (_isLoading)
                throw new InvalidOperationException("Scene load already in progress.");
            
            if (_loadedScenes.Contains(sceneName))
                return;

            
            _isLoading = true;

            var operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName.ToString(), LoadSceneMode.Additive);

            if (operation != null) 
                operation.allowSceneActivation = true;
            
            while (operation is { isDone: false })
                await Task.Yield();

            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName.ToString());

            if (!scene.IsValid())
                throw new Exception($"Failed to load scene: {sceneName}");

            _loadedScenes.Add(sceneName);

            if (setActive)
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene);
            
            OnSceneLoaded?.Invoke(sceneName);

            _isLoading = false;
        }

        public async Task UnloadSceneAsync(NamedScene sceneName)
        {
            if (_isLoading)
                throw new InvalidOperationException("Scene load/unload already in progress.");

            if (!_loadedScenes.Contains(sceneName))
                return;

            _isLoading = true;

            var operation = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName.ToString());

            while (operation is { isDone: false })
                await Task.Yield();

            _loadedScenes.Remove(sceneName);
            
            OnSceneUnloaded?.Invoke(sceneName);

            _isLoading = false;
        }

        public async Task SwitchToSceneAsync(NamedScene sceneName)
        {
            if (_isLoading)
                throw new InvalidOperationException("Scene transition already in progress.");

            _isLoading = true;

            var scenesToUnload = new List<NamedScene>(_loadedScenes);

            foreach (var loaded in scenesToUnload)
                await UnloadSceneAsync(loaded);
            

            await LoadSceneAdditiveAsync(sceneName);

            _isLoading = false;
        }

        public string ProcessName => "Scenes";

        public async Task InitializeForScene(
            SceneProfile sceneProfile,
            Action<int> declareSubprocessesCount,
            Action<int> declareStepsCallBack,
            Action<string> declareStep
        )
        {
            if (!sceneProfile)
                throw new ArgumentNullException(nameof(sceneProfile));

            var targetScenes = new List<NamedScene>(sceneProfile.subScenes);
            targetScenes.Insert(0, sceneProfile.sceneName);

            CollectionUtils.CompareCollections(
                _loadedScenes,
                targetScenes,
                out var scenesToUnload,
                out var scenesToLoad
            );

            var totalOperations = scenesToUnload.Count + scenesToLoad.Count;

            if (totalOperations == 0)
                return;

            declareSubprocessesCount(totalOperations);

            foreach (var scene in scenesToUnload)
            {
                declareStepsCallBack(1);

                await UnloadSceneAsync(scene);

                declareStep($"Unloaded {scene}");
            }

            foreach (var scene in scenesToLoad)
            {
                declareStepsCallBack(1);

                await LoadSceneAdditiveAsync(scene);

                declareStep($"Loaded {scene}");
            }
        }
    }
}