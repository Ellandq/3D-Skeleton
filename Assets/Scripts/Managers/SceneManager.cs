using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using Utils.Collections;
using Utils.Contract;
using Utils.Data.Save;
using Utils.Data.Scene;
using Utils.Enum;

namespace Managers
{
    public class SceneManager : ManagerBase<SceneManager>, IAsyncInitializable
    {
        private readonly HashSet<NamedScene> _loadedScenes = new();
        private bool _isLoading;

        public bool IsLoading => _isLoading;
        public IReadOnlyCollection<NamedScene> LoadedScenes => _loadedScenes;

        private SceneProfile _activeSceneProfile;

        private readonly List<Func<NamedScene, UniTask>> _preUnloadHooks = new();
        private readonly List<Func<NamedScene, UniTask>> _postUnloadHooks = new();

        public void RegisterPreUnload(Func<NamedScene, UniTask> hook)
            => _preUnloadHooks.Add(hook);

        public void RegisterPostUnload(Func<NamedScene, UniTask> hook)
            => _postUnloadHooks.Add(hook);

        public async UniTask LoadSceneAdditiveAsync(NamedScene sceneName, bool setActive = true)
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
        }

        public async UniTask UnloadSceneAsync(NamedScene sceneName)
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
            SceneProfile sceneProfile,
            Action<int> declareSubprocessesCount,
            Action<int> declareStepsCallBack,
            Action<string> declareStep)
        {
            if (!sceneProfile)
                throw new ArgumentNullException(nameof(sceneProfile));

            _activeSceneProfile = sceneProfile;

            var targetScenes = new List<NamedScene>(_activeSceneProfile.subScenes);
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

        public static SceneProfile GetCurrentProfile()
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

        public static void ApplyCanLoadWithoutLoadingScreen(SaveData saveData)
        {
            saveData.useLoadingScreen =
                GetLoadedScenes()
                    .ToHashSet()
                    .SetEquals(saveData.sceneSaveData.Select(s => s.scene));
        }
    }
}