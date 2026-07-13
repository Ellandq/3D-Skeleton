using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Model.Data.Save;
using Model.Data.Scene;
using UnityEngine.SceneManagement;
using Utils.Collections;
using Utils.Contract;
using Utils.Enum;

namespace Managers
{
    public class SceneManager : ManagerBase<SceneManager>, IAsyncInitializable
    {
        private readonly HashSet<NamedScene> _loadedScenes = new();
        private bool _isLoading;

        public bool IsLoading => _isLoading;
        public IReadOnlyCollection<NamedScene> LoadedScenes => _loadedScenes;

        private RuntimeSceneProfile _activeSceneProfile;

        public static event Action<NamedScene> OnSceneLoaded;
        public static event Action<NamedScene> OnSceneDeloaded;

        private readonly List<Func<NamedScene, UniTask>> _preUnloadHooks = new();
        private readonly List<Func<NamedScene, UniTask>> _postUnloadHooks = new();

        public static void RegisterPreUnload(Func<NamedScene, UniTask> hook)
            => Instance._preUnloadHooks.Add(hook);

        public static void RegisterPostUnload(Func<NamedScene, UniTask> hook)
            => Instance._postUnloadHooks.Add(hook);

        private async UniTask LoadSceneAdditiveAsync(NamedScene sceneName, bool setActive = true)
        {
            if (_isLoading)
                throw new InvalidOperationException("Scene load already in progress.");

            if (_loadedScenes.Contains(sceneName))
                return;

            _isLoading = true;

            var op = UnityEngine.SceneManagement.SceneManager
                .LoadSceneAsync(sceneName.ToString(), LoadSceneMode.Additive);

            if (op != null)
                op.allowSceneActivation = true;

            while (!op.isDone)
                await UniTask.Yield();

            var scene = UnityEngine.SceneManagement.SceneManager
                .GetSceneByName(sceneName.ToString());

            if (!scene.IsValid())
                throw new Exception($"Failed to load scene: {sceneName}");

            _loadedScenes.Add(sceneName);

            if (setActive)
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene);

            _isLoading = false;
            
            OnSceneLoaded?.Invoke(sceneName);
        }

        private async UniTask UnloadSceneAsync(NamedScene sceneName)
        {
            if (_isLoading)
                throw new InvalidOperationException("Scene load/unload already in progress.");

            if (!_loadedScenes.Contains(sceneName))
                return;

            _isLoading = true;

            foreach (var hook in _preUnloadHooks)
                await hook(sceneName);

            var op = UnityEngine.SceneManagement.SceneManager
                .UnloadSceneAsync(sceneName.ToString());

            while (op is { isDone: false })
                await UniTask.Yield();

            _loadedScenes.Remove(sceneName);

            foreach (var hook in _postUnloadHooks)
                await hook(sceneName);

            _isLoading = false;
            
            OnSceneDeloaded?.Invoke(sceneName);
        }

        public async UniTask SwitchToSceneAsync(NamedScene sceneName)
        {
            if (_isLoading)
                throw new InvalidOperationException("Scene transition already in progress.");

            _isLoading = true;

            var scenesToUnload = _loadedScenes.ToList();

            foreach (var scene in scenesToUnload)
                await UnloadSceneAsync(scene);

            await LoadSceneAdditiveAsync(sceneName);

            _isLoading = false;
        }

        public string ProcessName => "Scenes";

        public async UniTask InitializeForScene(
            RuntimeSceneProfile sceneProfile,
            Action<int> declareSubprocessesCount,
            Action<int> declareStepsCallBack,
            Action<string> declareStep)
        {
            _activeSceneProfile = sceneProfile;

            var targetScenes = new List<NamedScene>(_activeSceneProfile.SubScenes);
            targetScenes.Insert(0, _activeSceneProfile.sceneName);

            CollectionUtils.CompareCollections(
                _loadedScenes,
                targetScenes,
                out var scenesToUnload,
                out var scenesToLoad
            );

            var total = scenesToUnload.Count + scenesToLoad.Count;

            if (total == 0)
                return;

            declareSubprocessesCount(total);

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

        public static RuntimeSceneProfile GetCurrentProfile()
            => Instance._activeSceneProfile;

        public static List<NamedScene> GetLoadedScenes()
        {
            var count = UnityEngine.SceneManagement.SceneManager.loadedSceneCount;
            var result = new List<NamedScene>();

            for (var i = 0; i < count; i++)
            {
                var name = (NamedScene)Enum.Parse(
                    typeof(NamedScene),
                    UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).name
                );

                if (name == NamedScene.Bootstrap)
                    continue;

                result.Add(name);
            }

            return result;
        }

        public static bool CanLoadWithoutLoadingScreen(List<NamedScene> scenesToLoad)
        {
            return GetLoadedScenes()
                    .ToHashSet()
                    .SetEquals(scenesToLoad);
        }
    }
}