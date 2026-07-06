using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Model.Data.Scene
{
    [CreateAssetMenu(menuName = "Scene/AssetDataRe")]
    public class SceneAssetData : ScriptableObject
    {
        public List<PropCollection> collections = new();

        public Dictionary<string, PropCollection> ToDictionary()
            => collections.ToDictionary(c => c.assetAddress);
    }
}