using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Model.Data.Registry;
using Model.Data.Save;
using Model.Data.Scene;
using Model.Enum.Misc;
using UnityEngine;
using Utils.Contract;
using Utils.Enum;

namespace Managers
{
    public class SaveManager : ManagerBase<SaveManager>, IAsyncInitializable
    {
        [Header("Location Settings")]
        [SerializeField] private string saveLocation = "";

        [Header("Hooks")]
        private DiffRegistry registry;

        public static DiffRegistry Registry => Instance.registry;

        [Header("Save Data")]
        [SerializeField] private SaveData runtimeSave;

        private string selectedSave;


        private void Start()
        {
            registry = new DiffRegistry();

            saveLocation = Application.isEditor
                ? Path.Combine(Application.dataPath, "../Saves")
                : Path.Combine(Application.persistentDataPath, "Saves");
        }


        public async UniTask SaveGameAsync(SaveType type)
        {
            if (Time.timeScale != 0f)
                await UniTask.WaitForFixedUpdate();

            runtimeSave = registry.BuildSaveData();

            await WriteToDiskAsync(runtimeSave, type);
        }


        private async UniTask WriteToDiskAsync(
            SaveData saveData,
            SaveType type)
        {
            var json = JsonUtility.ToJson(saveData);

            if (!Directory.Exists(saveLocation))
                Directory.CreateDirectory(saveLocation);

            var filePath = Path.Combine(
                saveLocation,
                $"{type}_{saveData.timeStamp:yyyyMMdd_HHmmss}.txt");

            await File.WriteAllTextAsync(filePath, json);
        }


        public string ProcessName => "Save Manager";


        public async UniTask InitializeForScene(
            RuntimeSceneProfile sceneProfile,
            Action<int> declareSubprocessesCount,
            Action<int> declareStepsCallBack,
            Action<string> declareStep)
        {
            await LoadSave(
                sceneProfile.sceneName,
                declareSubprocessesCount,
                declareStepsCallBack,
                declareStep);
        }


        private async UniTask LoadSave(
            NamedScene scene,
            Action<int> declareSubprocessesCount,
            Action<int> declareStepsCallBack,
            Action<string> declareStep)
        {
            if (string.IsNullOrWhiteSpace(selectedSave))
            {
                declareSubprocessesCount.Invoke(1);
                declareStepsCallBack.Invoke(1);
                declareStep.Invoke("No save file to load - Skipping...");
                return;
            }

            var path = Path.Combine(
                saveLocation,
                $"{selectedSave}.txt");

            if (!File.Exists(path))
                return;

            var json = await File.ReadAllTextAsync(path);

            runtimeSave = JsonUtility.FromJson<SaveData>(json);

            registry.Initialize(
                runtimeSave,
                declareSubprocessesCount,
                declareStepsCallBack,
                declareStep);
        }


        public async UniTask FetchSaveNamesAsync(
            int batchSize,
            Func<IReadOnlyList<string>, bool> onBatch)
        {
            if (!Directory.Exists(saveLocation))
                return;

            var files = await UniTask.RunOnThreadPool(() =>
                Directory.GetFiles(saveLocation, "*.txt")
                    .Select(Path.GetFileNameWithoutExtension)
                    .OrderByDescending(x => x)
                    .ToList());

            for (var i = 0; i < files.Count; i += batchSize)
            {
                var batch = files
                    .Skip(i)
                    .Take(batchSize)
                    .ToList();

                if (!onBatch(batch))
                    return;

                await UniTask.Yield();
            }
        }


        public static string GetMostRecentSaveName()
        {
            if (!Directory.Exists(Instance.saveLocation))
                return null;

            return Directory
                .GetFiles(Instance.saveLocation, "*.txt")
                .Select(Path.GetFileNameWithoutExtension)
                .OrderByDescending(x => x)
                .FirstOrDefault();
        }


        public static void SetSelectedSave(string saveName)
        {
            Instance.selectedSave = saveName;
        }


        public static string GetSelectedSave()
        {
            return Instance.selectedSave;
        }
    }
}