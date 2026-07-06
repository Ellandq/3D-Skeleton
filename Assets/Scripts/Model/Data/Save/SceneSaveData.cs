using System;
using System.Collections.Generic;
using Model.Enum.Named;
using Utils.Enum;

namespace Model.Data.Save
{
    [Serializable]
    public class SceneSaveData
    {
        public NamedScene scene;
        public List<SavedProps> collections = new();
    }
}