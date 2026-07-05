using System;
using System.Collections.Generic;
using System.Linq;

namespace Utils.Data.Scene
{
    [Serializable]
    public class PropCollection
    {
        public string assetAddress;
        public List<PropData> propsData = new();
        public List<DynamicPropData> dynamicProps = new();

        public Dictionary<string, DynamicPropData> GetDynamicPropDict() =>
            dynamicProps.ToDictionary(p => p.id, p => p);
        
        public PropCollection Clone()
        {
            return new PropCollection
            {
                assetAddress = assetAddress,
                propsData = propsData.Select(p => p.Clone()).ToList(),
                dynamicProps = dynamicProps.Select(p => p.CloneDynamic()).ToList()
            };
        }
    }
}