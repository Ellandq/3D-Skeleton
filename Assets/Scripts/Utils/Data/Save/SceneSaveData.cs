using System;
using System.Collections.Generic;
using Utils.Enum;

namespace Utils.Data.Save
{
    [Serializable]
    public class SceneSaveData
    {
        public NamedScene scene;
        public List<SavedProps> collections;
    }
}