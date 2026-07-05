using System;
using System.Collections.Generic;
using Utils.Data.Scene;

namespace Utils.Data.Save
{
    [Serializable]
    public class SavedProps
    {
        public string assetAddress;
        public List<(DynamicPropData data, PropSceneStatus status)> dynamicProps = new();
    }
}