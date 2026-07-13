    using System;
    using System.Collections.Generic;

    namespace Model.Data.Scene
    {
        [Serializable]
        public class PropCollection
        {
            public string assetAddress;
            public List<PropData> props = new();
            public List<DynamicPropData> dynamicProps = new();
        }
    }