using System;
using System.Collections;
using UnityEngine;

namespace UserInterface.Windows
{
    public abstract class AnimatedWindowBase : WindowBase
    {
        [Header("Animations")] 
        [SerializeField] private Animator animator;
        [SerializeField] private string openTrigger = "Open"; 
        [SerializeField] private string closeTrigger = "Close";
        private Action _onAnimationFinish;

        protected override IEnumerator OpenAnimation(Action onActivate = null)
        {
            if (!animator)
            {
                yield return base.OpenAnimation(onActivate);
                yield break;
            }

            _onAnimationFinish = onActivate;

            animator.ResetTrigger(closeTrigger);
            animator.SetTrigger(openTrigger);
        }

        protected override IEnumerator CloseAnimation(Action onDeactivate = null)
        {
            if (!animator)
            {
                yield return base.CloseAnimation(onDeactivate);
                yield break;
            }

            _onAnimationFinish = onDeactivate;

            animator.ResetTrigger(openTrigger);
            animator.SetTrigger(closeTrigger);
        }

        public void ActivateCallBack()
        {
            _onAnimationFinish?.Invoke();
        }
    }
}