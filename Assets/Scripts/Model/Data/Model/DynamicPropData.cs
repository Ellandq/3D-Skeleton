using System;
using UnityEngine;

namespace Model.Data.Model
{
    [Serializable]
    public class DynamicPropData : PropData
    {
        public float mass;
        public RigidbodyInterpolation interpolation;
        public CollisionDetectionMode collisionDetectionMode;
        public Vector3 velocity;
        public Vector3 angularVelocity;
        public bool usesGravity;
        public bool isKinematic;
    }
}