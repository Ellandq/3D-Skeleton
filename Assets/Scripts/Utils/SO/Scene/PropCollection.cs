using System;
using System.Collections.Generic;

namespace Utils.SO.Scene
{
    [Serializable]
    public class PropCollection
    {
        public string assetAddress;
        public List<PropData> propsData = new();
        public List<DynamicPropData> dynamicProps = new();
    }
}