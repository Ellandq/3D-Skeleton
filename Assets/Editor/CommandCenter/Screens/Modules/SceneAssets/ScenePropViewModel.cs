using UnityEngine;

namespace Editor.CommandCenter.Screens.Modules.SceneAssets
{
    public class ScenePropViewModel
    {
        // General
        public GameObject instance;
        public string displayName;
        public string address;
        public string id;
        public string saveData;
        public bool isSaved;
        public bool isModified;
        public bool isDynamic;
        
        // PropData
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        
        // Dynamic
        public float mass;
        public RigidbodyInterpolation interpolation;
        public CollisionDetectionMode collisionDetectionMode;
        public Vector3 velocity;
        public Vector3 angularVelocity;
        public bool usesGravity;
        public bool isKinematic;
    }
}