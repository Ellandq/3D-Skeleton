using System;
using UnityEngine;

namespace Utils.Data.Scene
{
    [Serializable]
    public class PropData
    {
        public string id;
        public string saveData;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        
        public virtual PropData Clone()
        {
            return new PropData
            {
                id = id,
                saveData = saveData,
                position = position,
                rotation = rotation,
                scale = scale
            };
        }
    }
}