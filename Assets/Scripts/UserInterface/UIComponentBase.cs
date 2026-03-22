using System;
using System.Collections;
using UnityEngine;

namespace UserInterface
{
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(Animator))]
    public abstract class UIComponentBase : MonoBehaviour, IUIComponent
    {
        [Header("Components")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Animator animator;
        
        [Header("Animation Settings")]
        [SerializeField] private string openState = "Open"; 
        [SerializeField] private string closeState = "Close";
        public bool IsOpening;
        public bool IsClosing;
            
        private Action _onAnimationFinish;
        
        [Header("Parameter hashes")]
        private static readonly int Speed = Animator.StringToHash("Speed");
        
        public virtual void Activate(bool instant, Action onActivate = null)
        {
            if (instant)
            {
                ChangeComponentState(true);
                _onAnimationFinish = onActivate;
                IsOpening = true;
                IsClosing = false;
                ForceAnimationFinish(openState);
                return;
            }

            if (IsOpening)
            {
                return;
            }
            
            IsOpening = true;
            _onAnimationFinish = onActivate;
            
            if (IsClosing)
            {
                IsClosing = false;
                StartCoroutine(ReverseAnimation());
                return;
            }
            
            ChangeComponentState(true);
            StartCoroutine(RunAnimation(openState));
        }

        public virtual void Deactivate(bool instant, Action onDeactivate = null)
        {
            onDeactivate += () => ChangeComponentState(false);
            if (instant)
            {
                _onAnimationFinish = onDeactivate;
                IsOpening = false;
                IsClosing = true;
                ForceAnimationFinish(closeState);
                return;
            }
            
            if (IsClosing)
            {
                return;
            }
            
            IsClosing = true;
            _onAnimationFinish = onDeactivate;
            
            if (IsOpening)
            {
                IsOpening = false;
                StartCoroutine(ReverseAnimation());
                return;
            }
            
            StartCoroutine(RunAnimation(closeState));
        }

        protected virtual void ChangeComponentState(bool active)
        {
            gameObject.SetActive(active);
        }

        protected void ForceAnimationFinish(string stateKey)
        {
            StopAllCoroutines();
            animator.SetFloat(Speed, 1f);
            animator.Play(stateKey, 0, 1f);
            animator.Update(0f);
            _onAnimationFinish?.Invoke();
        }
        
        protected virtual IEnumerator ReverseAnimation()
        {
            const float duration = 0.1f;
            const float targetSpeed = -1f;
            
            var time = 0f;
            var startSpeed = animator.GetFloat(Speed);
            
            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                var t = time / duration;

                t *= t;

                var newSpeed = Mathf.Lerp(startSpeed, targetSpeed, t);
                animator.SetFloat(Speed, newSpeed);

                yield return null;
            }
            
            animator.SetFloat(Speed, targetSpeed);
        }

        protected virtual IEnumerator RunAnimation(string stateKey)
        {
            animator.SetFloat(Speed, 1f);
            animator.Play(stateKey, 0, 0f);
            animator.Update(0f);

            while (true)
            {
                var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                var speed = animator.GetFloat(Speed);

                if (speed < 0f && stateInfo.normalizedTime <= 0f)
                {
                    break;
                }

                if (stateInfo.IsName(stateKey) && stateInfo.normalizedTime >= 1f)
                {
                    break;
                }

                yield return null;
            }

            FinishAnimation();
        }
        
        private void FinishAnimation()
        {
            IsOpening = false;
            IsClosing = false;

            animator.SetFloat(Speed, 1f);

            _onAnimationFinish?.Invoke();
            _onAnimationFinish = null;
        }

        protected void EnableInteractions()
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        protected void DisableInteractions()
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}