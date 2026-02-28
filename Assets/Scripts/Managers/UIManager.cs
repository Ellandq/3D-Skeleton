using System;
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

        public void ActivateComponent<T>(T type, bool instant = false) where T : Enum
        {
            if (typeof(T) == typeof(NamedHUD))
            {
                var key = (NamedHUD)(object)type;
                if (_huds.TryGetValue(key, out var hud))
                {
                    // TODO
                }
            }
            else if (typeof(T) == typeof(NamedOverlay))
            {
                var key = (NamedOverlay)(object)type;
                if (_overlays.TryGetValue(key, out var overlay))
                {
                    // TODO
                }
            }
            else if (typeof(T) == typeof(NamedScreen))
            {
                var key = (NamedScreen)(object)type;
                if (_screens.TryGetValue(key, out var screen))
                {
                    // TODO
                }
            }
            else
            {
                throw new ArgumentException("Unsupported enum type: " + typeof(T));
            }
        }
    }
}