using System.Collections.Generic;
using UnityEngine;

namespace Utils.SO
{
    [CreateAssetMenu(menuName = "Scenes/Scene Profile")]
    public class SceneProfile : ScriptableObject
    {
        public List<string> preloadKeys = new();
    }
}