using System.Collections.Generic;
using UnityEngine;
using UserInterface.HUD;
using UserInterface.Overlay;
using UserInterface.Screen;
using Utils.Enum;

namespace Utils.SO
{
    [CreateAssetMenu(menuName = "Scenes/Scene Profile")]
    public class SceneProfile : ScriptableObject
    {
        [Header("Scene Settings")]
        public NamedScene sceneName;
        public List<NamedScene> subScenes = new();
        public bool useLoadScreen;
        
        [Header("UI Assets")]
        public List<NamedHUD> hudKeys = new();
        public List<NamedOverlay> overlayKeys = new();
        public List<NamedScreen> screenKeys = new();
    }
}