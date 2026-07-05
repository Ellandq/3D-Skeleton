using System;
using System.Collections.Generic;
using System.Linq;
using Utils.Enum;

namespace Utils.Data.Save
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
            var dict = sceneSaveData.ToDictionary(s => s.scene);

            foreach (var s in incoming)
                dict[s.scene] = s;

            sceneSaveData = new List<SceneSaveData>(dict.Values);
        }
    }
}