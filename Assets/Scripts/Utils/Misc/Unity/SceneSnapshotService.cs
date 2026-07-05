using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Managers;
using Utils.Data.Save;
using Utils.Enum;

namespace Utils.Misc.Unity
{
    public static class SceneSnapshotService
    {
        public static async UniTask<List<SceneSaveData>> CaptureAsync(List<NamedScene> scenes)
        {
            await UniTask.WaitForFixedUpdate();

            var result = new List<SceneSaveData>(scenes.Count);
            result.AddRange(scenes.Select(CaptureScene));

            return result;
        }

        private static SceneSaveData CaptureScene(NamedScene scene)
        {
            return AssetManager.GetSceneAssetDataChangeListInternal(scene);
        }
    }
}