using System;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UserInterface.Components;

namespace UserInterface.Windows
{
    public class ExitConfirmationWindow : WindowBase
    {
        [Header("Components")] 
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private TMP_Text message;
        [SerializeField] private OutsideClickDetector outsideClickDetector;

        private void Awake()
        {
            outsideClickDetector = UIManager.Instance.OutsideClickDetectorRef;
            message.text = "Are you sure you want to exit?";
            confirmButton.onClick.AddListener(Application.Quit);
            cancelButton.onClick.AddListener(() => Deactivate(false));
        }

        public override void Activate(bool instant, Action onActivate = null)
        {
            outsideClickDetector.Subscribe(() => Deactivate(false));
            onActivate += () => outsideClickDetector.gameObject.SetActive(true);
            base.Activate(instant, onActivate);
        }
        
        public override void Deactivate(bool instant, Action onDeactivate = null)
        {
            outsideClickDetector.Unsubscribe(() => Deactivate(false));
            onDeactivate += () => outsideClickDetector.gameObject.SetActive(false);
            base.Deactivate(instant, onDeactivate);
        }

        private void OnDestroy()
        {
            outsideClickDetector.Unsubscribe(() => Deactivate(false));
        }
    }
}