using System;
using Model.Data.Model;

namespace Model.Data.Save
{
    [Serializable]
    public class SavedPropChange
    {
        public DynamicPropData data;
        public PropSceneStatus status;
    }
}