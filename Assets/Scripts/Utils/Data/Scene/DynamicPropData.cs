using System;
using UnityEngine;

namespace Utils.Data.Scene
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
        
        public DynamicPropData CloneDynamic()
        {
            return new DynamicPropData
            {
                id = id,
                saveData = saveData,
                position = position,
                rotation = rotation,
                scale = scale,

                mass = mass,
                interpolation = interpolation,
                collisionDetectionMode = collisionDetectionMode,
                velocity = velocity,
                angularVelocity = angularVelocity,
                usesGravity = usesGravity,
                isKinematic = isKinematic
            };
        }
    }
}