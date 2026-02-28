using System.Collections.Generic;
using UnityEngine;
using UserInterface.HUD;
using UserInterface.Overlay;
using UserInterface.Screen;

namespace Managers
{
    public class UIManager : ManagerBase<UIManager>
    {
        [Header("UI Components")] 
        // HUDs
        private Dictionary<NamedHUD, IHUD> _huds;
        [SerializeField] private List<HUDBase> huds; 
        
        // Overlays
        private Dictionary<NamedOverlay, IOverlay> _overlays;
        [SerializeField] private List<OverlayBase> overlays;
        
        // Screens
        private Dictionary<NamedScreen, IScreen> _screens;
        [SerializeField] private List<ScreenBase> screens;
    }
}