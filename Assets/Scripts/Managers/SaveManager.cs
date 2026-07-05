#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Utils.Contract;
using Utils.Data.Save;
using Utils.Data.Scene;
using Utils.Enum;
using Utils.Enum.Misc;
using Utils.Misc.Unity;

namespace Managers
{
    public class SaveManager : ManagerBase<SaveManager>, IAsyncInitializable
    {
        [Header("Save Data")]
        [SerializeField] private SaveData? mostRecentSave;
        private SaveData? _runtimeSave;
        private string? selectedSave;

        [Header("Location Settings")]
        [SerializeField] private string saveLocation = "";

        protected override void Awake()
        {
            base.Awake();

            saveLocation = Application.isEditor
                ? Path.Combine(Application.dataPath, "../Saves")
                : Path.Combine(Application.persistentDataPath, "Saves");

            SceneManager.Instance.RegisterPreUnload(OnPreSceneUnload);
        }

        private async UniTask OnPreSceneUnload(NamedScene scene)
        {
            var snapshot = await SceneSnapshotService.CaptureAsync(
                new List<NamedScene> { scene }
            );

            _runtimeSave ??= new SaveData();

            _runtimeSave.MergeSceneSaveData(snapshot);
        }

        public async UniTask SaveGameAsync(SaveType type)
        {
            var loadedScenes = SceneManager.GetLoadedScenes();

            var snapshot = await SceneSnapshotService.CaptureAsync(loadedScenes);

            _runtimeSave ??= new SaveData();

            _runtimeSave.timeStamp = DateTimeOffset.Now;
            _runtimeSave.MergeSceneSaveData(snapshot);
            mostRecentSave = _runtimeSave;

            await WriteToDiskAsync(_runtimeSave, type);
        }

        private async UniTask WriteToDiskAsync(SaveData saveData, SaveType type)
        {
            var json = JsonUtility.ToJson(saveData);

            if (!Directory.Exists(saveLocation))
                Directory.CreateDirectory(saveLocation);

            var filePath = Path.Combine(
                saveLocation,
                $"{type}_{saveData.timeStamp:yyyyMMdd_HHmmss}.txt"
            );

            await File.WriteAllTextAsync(filePath, json);
        }

        public string ProcessName => "Save Manager";

        public async UniTask InitializeForScene(
            SceneProfile sceneProfile, 
            Action<int> declareSubprocessesCount, 
            Action<int> declareStepsCallBack,
            Action<string> declareStep)
        {
            declareSubprocessesCount.Invoke(2);
            declareStepsCallBack.Invoke(1);
            
            declareStep.Invoke("Loading save");
            await LoadSave(selectedSave);

            if (mostRecentSave == null)
            {
                declareStepsCallBack.Invoke(1);
                declareStep.Invoke("Couldn't load save file - Skipping...");
                return;
            }

            declareStepsCallBack.Invoke(2);
            declareStep.Invoke("Applying save data");
            sceneProfile.ApplySaveData(mostRecentSave);
            declareStep.Invoke("Save data applied");
        }
        
        private static async UniTask LoadSave(string? fileName = null)
        {
            var path = fileName ?? GetMostRecentSaveFile();
            var json = await File.ReadAllTextAsync(path);
            Instance.mostRecentSave = JsonUtility.FromJson<SaveData>(json);
        }
        
        private static string? GetMostRecentSaveFile()
        {
            if (!Directory.Exists(Instance.saveLocation))
                return null;

            return new DirectoryInfo(Instance.saveLocation)
                .GetFiles("*.txt")
                .OrderByDescending(f => f.LastWriteTimeUtc)
                .FirstOrDefault()
                ?.FullName;
        }
    }
}