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
        
        public virtual void Activate(bool instant)
        {
            StopAllCoroutines();
            if (instant)
            {
                canvasGroup.alpha = 1f;
                gameObject.SetActive(true);
                return;
            }
            StartCoroutine(OpenAnimation());
        }

        public virtual void Deactivate(bool instant)
        {
            StopAllCoroutines();
            if (instant)
            {
                canvasGroup.alpha = 0f;
                gameObject.SetActive(false);
                return;
            }
            StartCoroutine(CloseAnimation());
        }

        protected virtual IEnumerator OpenAnimation()
        {
            return FadeRoutine(1f);
        }

        protected virtual IEnumerator CloseAnimation()
        {
            return FadeRoutine(0f);
        }
        
        private IEnumerator FadeRoutine(float targetAlpha)
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
        }
    }
}