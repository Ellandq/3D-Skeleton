using System;
using Model.Data.Scene;

namespace Model.Data.Save
{
    [Serializable]
    public class SavedPropChange
    {
        public DynamicPropData data;
        public PropSceneStatus status;
    }
}