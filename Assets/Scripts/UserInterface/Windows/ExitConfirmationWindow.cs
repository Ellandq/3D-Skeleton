using System;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UserInterface.Components;
using UserInterface.Screen;
using Utils.Contract;

namespace UserInterface.Windows
{
    public class ExitConfirmationWindow : WindowBase, IUIStackable
    {
        public override NamedWindow Name => NamedWindow.ExitConfirmation;
        public override UIPriority Priority => UIPriority.High;
        
        private Action _outsideClickAction;
        
        [Header("Components")] 
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private TMP_Text message;
        [SerializeField] private OutsideClickDetector outsideClickDetector;

        private void Awake()
        {
            _outsideClickAction = () => Deactivate(false);
            outsideClickDetector = UIManager.Instance.OutsideClickDetectorRef;
            message.text = "Are you sure you want to exit?";
            confirmButton.onClick.AddListener(Application.Quit);
            cancelButton.onClick.AddListener(() => Deactivate(false));
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

        protected override void ChangeComponentState(bool active)
        {
            base.ChangeComponentState(active);
            if (active)
                return;
            UIManager.Instance.OnFinishPop(this);
        }

        #endregion
    }
}