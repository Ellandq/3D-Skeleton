using System;
using UnityEngine;

namespace Utils.SO.Scene
{
    [Serializable]
    public class DynamicPropData : PropData
    {
        public Vector3 velocity;
        public Vector3 angularVelocity;
        public bool usesGravity;
    }
}