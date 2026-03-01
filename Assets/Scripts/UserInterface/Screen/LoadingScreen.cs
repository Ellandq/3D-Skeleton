using System;
using System.Collections;
using TMPro;
using UnityEngine;
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
        [SerializeField] private float rotationSpeed = 180f;
        [SerializeField] private float barMoveSpeed = 2f;

        [Header("Loader")]
        private IProgressReporter _loader;

        [Header("Status")]
        private float _targetProgress;

        private Coroutine _loadingCoroutine;

        protected void Awake()
        {
            if (loadingBar.childCount > 0)
                maxWidth = ((RectTransform)loadingBar.GetChild(0)).rect.width;
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

            if (_loadingCoroutine != null)
                StopCoroutine(_loadingCoroutine);

            _loadingCoroutine = StartCoroutine(LoadingAction());
        }

        private void UpdateProgress(float progress)
        {
            _targetProgress = Mathf.Clamp01(progress);
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
            if (_loadingCoroutine == null) return;
            StopCoroutine(_loadingCoroutine);
            _loadingCoroutine = null;
        }

        private IEnumerator LoadingAction()
        {
            var y = loadingBar.sizeDelta.y;

            while (isActiveAndEnabled)
            {
                var dt = Time.unscaledDeltaTime;
                var targetWidth = Mathf.Lerp(minWidth, maxWidth * _targetProgress, 1f);
                var newWidth = Mathf.MoveTowards(loadingBar.sizeDelta.x, targetWidth, barMoveSpeed * maxWidth * dt);
                loadingBar.sizeDelta = new Vector2(newWidth, y);

                loadingIcon.Rotate(0f, 0f, -rotationSpeed * dt);

                yield return null;
            }
        }

        private void DeactivateAfterLoad(Action onComplete = null)
        {
            if (_loadingCoroutine != null)
                StopCoroutine(_loadingCoroutine);

            _loadingCoroutine = StartCoroutine(WaitUntilVisualFinish(() =>
            {
                base.Deactivate(false, onComplete);
            }));
        }

        private IEnumerator WaitUntilVisualFinish(Action onComplete)
        {
            var y = loadingBar.sizeDelta.y;

            while (Math.Abs(loadingBar.sizeDelta.x - maxWidth) > 0.01f)
            {
                var dt = Time.unscaledDeltaTime;
                var newWidth = Mathf.MoveTowards(loadingBar.sizeDelta.x, maxWidth, barMoveSpeed * maxWidth * dt);
                loadingBar.sizeDelta = new Vector2(newWidth, y);

                loadingIcon.Rotate(0f, 0f, -rotationSpeed * dt);

                yield return null;
            }

            onComplete?.Invoke();
        }

        public override void Deactivate(bool instant, Action onDeactivate = null)
        {
            if (instant)
            {
                if (_loadingCoroutine != null)
                {
                    StopCoroutine(_loadingCoroutine);
                    _loadingCoroutine = null;
                }
                base.Deactivate(true, onDeactivate);
            }
            else
            {
                DeactivateAfterLoad(onDeactivate);
            }
        }
    }
}