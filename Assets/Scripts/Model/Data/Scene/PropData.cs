    using System;
    using UnityEngine;

    namespace Model.Data.Scene
    {
        [Serializable]
        public class PropData
        {
            public string id;
            public string saveData;
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;
        }
    }