using System;
using System.Collections.Generic;
using Model.Enum.Named;
using Utils.Enum;

namespace Model.Data.Save
{
    [Serializable]
    public class SaveData
    {
        public List<SceneSaveData> sceneSaveData = new();
        public bool useLoadingScreen;

        [NonSerialized]
        public DateTimeOffset timeStamp;

        public void MergeSceneSaveData(List<SceneSaveData> incoming)
        {
            var dict = new Dictionary<NamedScene, SceneSaveData>();

            foreach (var s in sceneSaveData)
                dict[s.scene] = s;

            foreach (var s in incoming)
                dict[s.scene] = s;

            sceneSaveData = new List<SceneSaveData>(dict.Values);
        }
    }
}