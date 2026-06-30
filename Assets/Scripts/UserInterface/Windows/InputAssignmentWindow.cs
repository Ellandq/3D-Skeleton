using System;
using Managers;
using Utils.Contract;

namespace UserInterface.Windows
{
    public class InputAssignmentWindow : WindowBase, IUIStackable
    {
        private static Action<string> _onInputRegistration;
        
        public static void Subscribe(Action<string> onInputRegistration) => _onInputRegistration += onInputRegistration;

        public override void Activate(bool instant, Action onActivate = null)
        {
            // TODO
            base.Activate(instant, onActivate);
        }
        
        #region UI STACK

        public void OnPush()
        {
            UIManager.Instance.BackgroundDim.Activate(false);
            EnableInteractions();
        }

        public void OnPushOther()
        {
            DisableInteractions();
        }

        public void OnPop()
        {
            UIManager.Instance.BackgroundDim.Deactivate(false);
            if (!IsClosing)
            {
                Deactivate(false);
                return;
            }
            Deactivate(true);
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