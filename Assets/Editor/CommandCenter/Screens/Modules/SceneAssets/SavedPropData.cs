using UnityEngine;

namespace Editor.CommandCenter.Screens.Modules.SceneAssets
{
    public class SavedPropData
    {
        public string address;
        public string id;

        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public float mass;
        public RigidbodyInterpolation interpolation;
        public CollisionDetectionMode collisionDetectionMode;
        public Vector3 velocity;
        public Vector3 angularVelocity;
        public bool usesGravity;
        public bool isKinematic;
    }
}