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

        public Vector3 velocity;
        public Vector3 angularVelocity;

        public bool usesGravity;

        public bool isDynamic;
    }
}