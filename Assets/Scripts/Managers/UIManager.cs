using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameInput;
using UnityEngine;
using UserInterface;
using UserInterface.Components;
using UserInterface.HUD;
using UserInterface.Overlay;
using UserInterface.Screen;
using UserInterface.Windows;
using Utils.Collections;
using Utils.Contract;
using Utils.SO;

namespace Managers
{
    public class UIManager : ManagerBase<UIManager>, IAsyncInitializable
    {
        [Header("HUD's")]
        [SerializeField] private Transform hudParent;
        private Dictionary<NamedHUD, HUDBase> _huds = new();
        [SerializeField] private List<HUDBase> huds; 
        
        [Header("Overlays")]
        [SerializeField] private Transform overlayParent;
        private Dictionary<NamedOverlay, OverlayBase> _overlays = new();
        [SerializeField] private List<OverlayBase> overlays;
        
        [Header("Screens")]
        [SerializeField] private Transform screenParent;
        private Dictionary<NamedScreen, ScreenBase> _screens = new();
        [SerializeField] private List<ScreenBase> screens;
        
        [Header("Windows")]
        [SerializeField] private Transform windowParent;
        private Dictionary<NamedWindow, WindowBase> _windows = new();
        [SerializeField] private List<WindowBase> windows;
        
        [Header("Utils")]
        [SerializeField] private OutsideClickDetector outsideClickDetector;
        public static OutsideClickDetector OutsideClickDetectorRef => Instance.outsideClickDetector;
        [SerializeField] private BackgroundDim backgroundDim;
        public static BackgroundDim BackgroundDim => Instance.backgroundDim;

        [Header("UI Stack")] 
        private readonly Stack<IUIStackable> _uiStack = new ();
        private Action _onEmptyStackExit;
        private bool _transitionLocked;

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
            
            _windows = windows.ToDictionary(
                window => window.Name,
                window => window
            );
        }

        private void Start()
        {
            InputManager.Instance.Subscribe(PlayerAction.Escape, state =>
            {
                if (state == ButtonState.Down)
                    PopUIStack(false);
                
            });
        }

        #region UTILS

        private void SortScreensByPriority()
        {
            var ordered = _screens.Values
                .Where(s => s)
                .OrderBy(s => s.Priority)
                .Reverse()
                .ToList();

            for (var i = 0; i < ordered.Count; i++)
            {
                if (ordered[i])
                    ordered[i].transform.SetSiblingIndex(i);
            }
        }

        #endregion

        #region UI STACK

        private void PushToUIStack(IUIStackable stackable, bool instant, Action onActivate = null)
        {
            if (_uiStack.TryPeek(out var component))
            {
                component.OnPushOther();
            }

            _uiStack.Push(stackable);
            stackable.OnPush(instant, onActivate);
        }

        private void PopUIStack(bool instant, Action onDeactivate = null)
        {
            if (_transitionLocked && !instant)
                return;
            if (_uiStack.TryPeek(out var component))
            {
                _transitionLocked = true;
                onDeactivate += () => OnFinishPop(component);
                component.OnPop(instant, onDeactivate);
            }
            else
            {
                _onEmptyStackExit?.Invoke();
            }
        }

        public static void CancelPop()
        {
            Instance._transitionLocked = false;
        }

        private void OnFinishPop(IUIStackable stackable)
        {
            _transitionLocked = false;
            if (_uiStack.TryPeek(out var component) && component == stackable)
            {
                _uiStack.Pop();
            }
            
            if (_uiStack.TryPeek(out var nextComponent))
            {
                nextComponent.OnPopOther();
            }
        }
        
        public static void SetOnEmptyStackExitCallback(Action action)
        {
            Instance._onEmptyStackExit = action;
        }

        #endregion

        #region COMPONENT CONTROL

        public static TC GetUIComponent<TE, TC>(TE type) where TE : Enum where TC : class, IUIComponent =>
            Instance.GetDeclaredUIComponent<TE, TC>(type);
        
        private TC GetDeclaredUIComponent<TE, TC>(TE type)
            where TE : Enum
            where TC : class, IUIComponent
        {
            if (typeof(TE) == typeof(NamedHUD))
            {
                var key = (NamedHUD)(object)type;
                return _huds.GetValueOrDefault(key) as TC;
            }

            if (typeof(TE) == typeof(NamedOverlay))
            {
                var key = (NamedOverlay)(object)type;
                return _overlays.GetValueOrDefault(key) as TC;
            }

            if (typeof(TE) != typeof(NamedScreen)) throw new ArgumentException("Unsupported enum type: " + typeof(TE));
            {
                var key = (NamedScreen)(object)type;
                return _screens.GetValueOrDefault(key) as TC;
            }

        }

        public static void ActivateComponent<T>(T type, bool instant = false, Action onActivate = null) where T : Enum
        {
            Instance.ChangeComponentState(type, true, instant, onActivate);
        }

        public static void DeactivateComponent<T>(T type, bool instant = false, Action onDeactivate = null) where T : Enum
        {
            Instance.ChangeComponentState(type, false, instant, onDeactivate);
        }

        private void ChangeComponentState<T>(T type, bool active, bool instant = false, Action onFinish = null) where T : Enum
        {
            IUIComponent component = null;

            if (typeof(T) == typeof(NamedHUD))
            {
                var key = (NamedHUD)(object)type;
                _huds.TryGetValue(key, out var comp);
                component = comp;
            }
            else if (typeof(T) == typeof(NamedOverlay))
            {
                var key = (NamedOverlay)(object)type;
                _overlays.TryGetValue(key, out var comp);
                component = comp;
            }
            else if (typeof(T) == typeof(NamedScreen))
            {
                var key = (NamedScreen)(object)type;
                _screens.TryGetValue(key, out var comp);
                component = comp;
            }
            else if (typeof(T) == typeof(NamedWindow))
            {
                var key = (NamedWindow)(object)type;
                _windows.TryGetValue(key, out var comp);
                component = comp;
            }
            else
            {
                throw new ArgumentException("Unsupported enum type: " + typeof(T));
            }

            switch (component)
            {
                case null:
                    return;
                case IUIStackable stackable when active:
                    PushToUIStack(stackable, instant, onFinish);
                    break;
                case IUIStackable:
                    PopUIStack(instant, onFinish);
                    break;
                default:
                {
                    if (active)
                        component.Activate(instant, onFinish);
                    else
                        component.Deactivate(instant, onFinish);
                    break;
                }
            }
        }
        
        #endregion

        #region ASYNC INITIALIZATION

        public string ProcessName => "UI";

        public async Task InitializeForScene(
            SceneProfile sceneProfile, 
            Action<int> declareSubprocessesCount,
            Action<int> declareStepsCallBack,
            Action<string> declareStep
        ) {
            declareSubprocessesCount.Invoke(4);
            await AddComponents(
                sceneProfile,
                declareStepsCallBack,
                declareStep
            );

            huds = _huds.Values.ToList();
            overlays = _overlays.Values.ToList();
            screens = _screens.Values.ToList();
            windows = _windows.Values.ToList();
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
            await AddComponents(profile.windowKeys, _windows, windowParent, declareStepsCallBack, declareStep);
            SortScreensByPriority();
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
                var obj = await assetManager.InstantiatePrefabAsync(key, parent, false);
                currentDict.Add(key, obj.GetComponent<TComp>());
            }
        }

        #endregion
    }
}