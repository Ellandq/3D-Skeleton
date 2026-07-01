using System;
using Managers;
using UnityEngine;
using UnityEngine.UI;
using UserInterface.Components;
using UserInterface.Screen;
using Utils.Contract;

namespace UserInterface.Windows.Settings
{
    public class RestoreDefaultSettingsWindow : WindowBase, IUIStackable
    {
        public override NamedWindow Name => NamedWindow.RestoreDefaultSettings;
        public override UIPriority Priority => UIPriority.High;
        
        private Action _outsideClickAction;
        
        [Header("Components")] 
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        private OutsideClickDetector outsideClickDetector;
        
        private void Awake()
        {
            _outsideClickAction = () => UIManager.DeactivateComponent(Name);
            outsideClickDetector = UIManager.OutsideClickDetectorRef;
            var settingsScreen = UIManager.GetUIComponent<NamedScreen, SettingsScreen>(NamedScreen.Settings);
            confirmButton.onClick.AddListener(() =>
            {
                settingsScreen.RestoreDefaults(true);
                UIManager.DeactivateComponent(Name);
            });
            cancelButton.onClick.AddListener(() => UIManager.DeactivateComponent(Name));
        }
        
        public override void Activate(bool instant, Action onActivate = null)
        {
            outsideClickDetector.Subscribe(_outsideClickAction);
            onActivate += () => outsideClickDetector.gameObject.SetActive(true);
            base.Activate(instant, onActivate);
        }
        
        public override void Deactivate(bool instant, Action onDeactivate = null)
        {
            outsideClickDetector.Unsubscribe(_outsideClickAction);
            onDeactivate += () =>
            {
                outsideClickDetector.gameObject.SetActive(false);
            };
            base.Deactivate(instant, onDeactivate);
        }

        private void OnDestroy()
        {
            outsideClickDetector.Unsubscribe(_outsideClickAction);
        }
        
        #region UI STACK

        public void OnPush(bool instant, Action onActivate = null)
        {
            if (!IsClosing)
            {
                Activate(instant, onActivate);
                return;
            }
            Activate(true, onActivate);
            EnableInteractions();
        }

        public void OnPushOther()
        {
            DisableInteractions();
        }

        public void OnPop(bool instant, Action onDeactivate = null)
        {
            if (!IsClosing)
            {
                Deactivate(instant, onDeactivate);
                return;
            }
            Deactivate(true, onDeactivate);
        }

        public void OnPopOther()
        {
            EnableInteractions();
        }

        #endregion
    }
}