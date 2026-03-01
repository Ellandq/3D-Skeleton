using System;
using System.Collections;
using UnityEngine;

namespace UserInterface
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIComponentBase : MonoBehaviour, IUIComponent
    {
        [Header("Components")]
        [SerializeField] private CanvasGroup canvasGroup;
        
        public virtual void Activate(bool instant, Action onActivate = null)
        {
            StopAllCoroutines();
            if (instant)
            {
                canvasGroup.alpha = 1f;
                gameObject.SetActive(true);
                return;
            }
            gameObject.SetActive(true);
            StartCoroutine(OpenAnimation(onActivate));
        }

        public virtual void Deactivate(bool instant, Action onDeactivate = null)
        {
            StopAllCoroutines();
            if (instant)
            {
                canvasGroup.alpha = 0f;
                gameObject.SetActive(false);
                onDeactivate?.Invoke();
                return;
            }
            StartCoroutine(CloseAnimation(onDeactivate));
        }

        protected virtual IEnumerator OpenAnimation(Action onActivate = null)
        {
            return FadeRoutine(1f, onActivate);
        }

        protected virtual IEnumerator CloseAnimation(Action onDeactivate = null)
        {
            return FadeRoutine(0f, onDeactivate);
        }
        
        private IEnumerator FadeRoutine(float targetAlpha, Action onFinish = null)
        {
            var startAlpha = canvasGroup.alpha;
            var time = 0f;
            var fadeDuration = Math.Abs(targetAlpha - startAlpha) * 0.5f;

            while (time < fadeDuration)
            {
                time += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
            canvasGroup.interactable = targetAlpha > 0;
            canvasGroup.blocksRaycasts = targetAlpha > 0;
            
            onFinish?.Invoke();
        }
    }
}