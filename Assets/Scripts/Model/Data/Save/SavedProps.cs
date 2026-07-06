using System;
using System.Collections.Generic;

namespace Model.Data.Save
{
    [Serializable]
    public class SavedProps
    {
        public string assetAddress;
        public List<SavedPropChange> changes = new();
    }
}