using System;
using System.Collections.Generic;
using Utils.Enum;

namespace Model.Data.Save
{
    [Serializable]
    public class SaveData
    {
        public List<SceneSaveData> sceneSaveData = new();

        [NonSerialized]
        public DateTimeOffset timeStamp;
    }
}