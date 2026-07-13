using Model.Data.Scene;
using UnityEngine;

namespace Utils.Props
{
    public static class DynamicPropUtility
    {
        public static DynamicPropData Clone(DynamicPropData source)
        {
            return new DynamicPropData
            {
                id = source.id,

                position = source.position,
                rotation = source.rotation,
                scale = source.scale,

                saveData = source.saveData,

                mass = source.mass,

                interpolation = source.interpolation,
                collisionDetectionMode = source.collisionDetectionMode,

                velocity = source.velocity,
                angularVelocity = source.angularVelocity,

                usesGravity = source.usesGravity,
                isKinematic = source.isKinematic
            };
        }
        
        public static bool SameData(
            DynamicPropData a,
            DynamicPropData b)
        {
            if (a == null || b == null)
                return false;

            return a.position == b.position &&
                   a.rotation == b.rotation &&
                   a.scale == b.scale &&
                   Equals(a.saveData, b.saveData) &&
                   a.velocity == b.velocity &&
                   a.angularVelocity == b.angularVelocity &&
                   Mathf.Approximately(a.mass, b.mass) &&
                   a.interpolation == b.interpolation &&
                   a.collisionDetectionMode == b.collisionDetectionMode &&
                   a.usesGravity == b.usesGravity &&
                   a.isKinematic == b.isKinematic;
        }
    }
}