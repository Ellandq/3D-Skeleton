using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Utils.Contract;

namespace UserInterface.Screen
{
    public class LoadingScreen : ScreenBase
    {
        public override NamedScreen Name => NamedScreen.Loading;
        
        [Header("Components")] 
        [SerializeField] private RectTransform loadingBar;
        [SerializeField] private RectTransform loadingIcon;
        [SerializeField] private TMP_Text messageDisplay;

        [Header("Settings")] 
        [SerializeField] private float minWidth;
        [SerializeField] private float maxWidth;
        [SerializeField] private float rotationSpeed = 90f;

        [Header("Loader")]
        private IProgressReporter _loader;
        
        [Header("Status")]
        private float _targetProgress;

        protected void Awake()
        {
            maxWidth = ((RectTransform)loadingBar.GetChild(0)).rect.width;
            Initialize();
        }
        
        private void Initialize()
        {
            loadingIcon.localRotation = Quaternion.identity;
            loadingBar.sizeDelta = new Vector2(minWidth, loadingBar.sizeDelta.y);
            messageDisplay.text = "";
        }

        public void Bind(IProgressReporter loader)
        {
            if (_loader != null)
            {
                _loader.OnProgress -= UpdateProgress;
                _loader.OnStepChanged -= UpdateMessage;
            }

            _loader = loader;
            
            _loader.OnProgress += UpdateProgress;
            _loader.OnStepChanged += UpdateMessage;

            StartCoroutine(LoadingAction());
        }

        private void UpdateProgress(float progress)
        {
            _targetProgress = progress;
        }

        private void UpdateMessage(string message)
        {
            messageDisplay.text = message;
        }

        private void OnEnable()
        {
            Initialize();
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private IEnumerator LoadingAction()
        {
            var bar = loadingBar.sizeDelta;
            var y = bar.y;
            
            while (isActiveAndEnabled)
            {
                var time = Time.unscaledDeltaTime;
                
                var target = new Vector2(maxWidth * _targetProgress, y);
                var moveTo = Vector2.Lerp(bar, target, time);
                loadingBar.sizeDelta = moveTo;

                var delta = rotationSpeed * time;
                loadingIcon.Rotate(0f, 0f, -delta);
                
                yield return null;
            }
        }
    }
}