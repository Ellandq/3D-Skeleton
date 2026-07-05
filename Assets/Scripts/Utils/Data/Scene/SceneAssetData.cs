using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utils.Data.Scene
{
    [CreateAssetMenu(menuName = "Scene/AssetData")]
    public class SceneAssetData : ScriptableObject
    {
        public List<PropCollection> collections = new();

        public Dictionary<string, Dictionary<string, DynamicPropData>> GetCollectionDictionary()
            => collections.ToDictionary(c => c.assetAddress, c => c.GetDynamicPropDict());
    }
}