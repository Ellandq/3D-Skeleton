using System.Collections.Generic;
using UnityEngine;
using UserInterface.HUD;
using UserInterface.Overlay;
using UserInterface.Screen;

namespace Utils.SO
{
    [CreateAssetMenu(menuName = "Scenes/Scene Profile")]
    public class SceneProfile : ScriptableObject
    {
        // UI
        public List<NamedHUD> hudKeys = new();
        public List<NamedOverlay> overlayKeys = new();
        public List<NamedScreen> screenKeys = new();
    }
}