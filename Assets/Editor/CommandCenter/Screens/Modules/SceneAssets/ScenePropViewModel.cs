using UnityEngine;

namespace Editor.CommandCenter.Screens.Modules.SceneAssets
{
    public class ScenePropViewModel
    {
        public string displayName;

        public string address;

        public string id;

        public bool isSaved;

        public bool isModified;

        public bool isDynamic;

        public Vector3 position;

        public Quaternion rotation;

        public Vector3 scale;

        public Vector3 velocity;

        public Vector3 angularVelocity;

        public bool usesGravity;

        public GameObject instance;
    }
}