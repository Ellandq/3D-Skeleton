using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UserInterface.HUD;
using UserInterface.Overlay;
using UserInterface.Screen;
using Utils.Collections;
using Utils.Contract;
using Utils.Enum;
using Utils.SO;

namespace Managers
{
    public class UIManager : ManagerBase<UIManager>, IAsyncInitializable
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

        #region COMPONENT CONTROL

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

        #endregion

        #region ASYNC INITIALIZATION
        
        public async Task InitializeForScene(
            SceneProfile sceneProfile, 
            Action<int> declareSubprocessesCount,
            Action<int> declareStepsCallBack,
            Action<string> declareStep
        ) {
            declareSubprocessesCount.Invoke(3);
            await AddComponents(
                sceneProfile,
                declareStepsCallBack,
                declareStep
            );
        }

        private async Task AddComponents(
            SceneProfile profile, 
            Action<int> declareStepsCallBack,
            Action<string> declareStep
        )
        {
            await AddComponents(profile.hudKeys, _huds, hudParent, declareStepsCallBack, declareStep);
            await AddComponents(profile.overlayKeys, _overlays, overlayParent, declareStepsCallBack, declareStep);
            await AddComponents(profile.screenKeys, _screens, screenParent, declareStepsCallBack, declareStep);
        }
        
        private static async Task AddComponents<TEnum, TComp>(
            List<TEnum> desiredKeys,
            Dictionary<TEnum, TComp> currentDict,
            Transform parent,
            Action<int> declareStepsCallBack,
            Action<string> declareStep
        ) where TEnum : Enum where TComp : Component
        {
            CollectionUtils.CompareListAndDictionary(
                currentDict,
                desiredKeys,
                out var onlyInList,
                out var onlyInDict
            );

            declareStepsCallBack.Invoke(onlyInDict.Count + onlyInList.Count);

            var assetManager = AssetManager.Instance;

            foreach (var key in onlyInDict)
            {
                declareStep.Invoke($"Removing {typeof(TComp).Name}: {key}");
                var obj = currentDict[key].gameObject;
                assetManager.ReleaseInstance(obj);
                currentDict.Remove(key);
            }

            foreach (var key in onlyInList)
            {
                declareStep.Invoke($"Adding {typeof(TComp).Name}: {key}");
                var obj = await assetManager.InstantiatePrefabAsync(key, parent);
                currentDict.Add(key, obj.GetComponent<TComp>());
            }
        }

        #endregion
    }
}