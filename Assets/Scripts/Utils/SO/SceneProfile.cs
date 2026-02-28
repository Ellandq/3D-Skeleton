using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UserInterface.HUD;
using UserInterface.Overlay;
using UserInterface.Screen;

namespace Utils.SO
{
    [CreateAssetMenu(menuName = "Scenes/Scene Profile")]
    public class SceneProfile : ScriptableObject
    {
        // General
        public List<string> preloadKeys = new();
        
        // UI
        public List<NamedHUD> hudKeys = new();
        public List<NamedOverlay> overlayKeys = new();
        public List<NamedScreen> screenKeys = new();

        public List<string> GetKeys()
        {
            return CombineLists(
                preloadKeys.Cast<object>(),
                hudKeys.Cast<object>(),
                overlayKeys.Cast<object>(),
                screenKeys.Cast<object>()
            );
        }

        private static List<string> CombineLists(params IEnumerable<object>[] lists)
        {
            return lists
                .SelectMany(list => list.Select(item => item.ToString()))
                .ToList();
        }
    }
}