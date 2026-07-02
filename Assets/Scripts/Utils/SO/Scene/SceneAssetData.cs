using System.Collections.Generic;
using UnityEngine;
using Utils.Enum;

namespace Utils.SO.Scene
{
    [CreateAssetMenu(menuName = "Scene/AssetData")]
    public class SceneAssetData : ScriptableObject
    {
        public List<PropCollection> collections = new();
    }
}