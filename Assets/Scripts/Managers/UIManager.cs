using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UserInterface.HUD;
using UserInterface.Overlay;
using UserInterface.Screen;
using Utils.Enum;

namespace Managers
{
    public class UIManager : ManagerBase<UIManager>
    {
        [Header("UI Components")]
        // HUDs
        [SerializeField] private Transform hudParent;
        private Dictionary<NamedHUD, HUDBase> _huds = new();
        [SerializeField] private List<HUDBase> huds; 
        
        // Overlays
        [SerializeField] private Transform overlayParent;
        private Dictionary<NamedOverlay, OverlayBase> _overlays = new();
        [SerializeField] private List<OverlayBase> overlays;
        
        // Screens
        [SerializeField] private Transform screenParent;
        private Dictionary<NamedScreen, ScreenBase> _screens = new();
        [SerializeField] private List<ScreenBase> screens;

        protected override void Awake()
        {
            base.Awake();
            
            _huds = huds.ToDictionary(
                hud => hud.Name,
                hud => hud
            );
            
            _overlays = overlays.ToDictionary(
                overlay => overlay.Name,
                overlay => overlay
            );
            
            _screens = screens.ToDictionary(
                screen => screen.Name,
                screen => screen
            );
        }

        public void InitializeForScene(NamedScene scene)
        {
            
        }

        public void ActivateComponent<T>(T type, bool instant = false) where T : Enum
        {
            ChangeComponentState(type, true, instant);
        }

        public void DeactivateComponent<T>(T type, bool instant = false) where T : Enum
        {
            ChangeComponentState(type, false, instant);
        }

        private void ChangeComponentState<T>(T type, bool active, bool instant = false) where T : Enum
        {
            if (typeof(T) == typeof(NamedHUD))
            {
                var key = (NamedHUD)(object)type;
                if (!_huds.TryGetValue(key, out var hud)) return;
                if (active) hud.Activate(instant);
                else hud.Deactivate(instant);
            }
            else if (typeof(T) == typeof(NamedOverlay))
            {
                var key = (NamedOverlay)(object)type;
                if (!_overlays.TryGetValue(key, out var overlay)) return;
                if (active) overlay.Activate(instant);
                else overlay.Deactivate(instant);
            }
            else if (typeof(T) == typeof(NamedScreen))
            {
                var key = (NamedScreen)(object)type;
                if (!_screens.TryGetValue(key, out var screen)) return;
                if (active) screen.Activate(instant);
                else screen.Deactivate(instant);
            }
            else
            {
                throw new ArgumentException("Unsupported enum type: " + typeof(T));
            }
        }
    }
}