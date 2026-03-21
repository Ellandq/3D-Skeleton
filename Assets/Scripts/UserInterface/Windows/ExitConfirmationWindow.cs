using System;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UserInterface.Components;
using UserInterface.Screen;

namespace UserInterface.Windows
{
    public class ExitConfirmationWindow : AnimatedWindowBase
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
            onDeactivate += () => outsideClickDetector.gameObject.SetActive(false);
            base.Deactivate(instant, onDeactivate);
        }

        private void OnDestroy()
        {
            outsideClickDetector.Unsubscribe(_outsideClickAction);
        }
    }
}