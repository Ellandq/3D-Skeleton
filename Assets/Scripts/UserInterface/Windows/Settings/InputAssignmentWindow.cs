using System;
using Managers;
using Utils.Contract;

namespace UserInterface.Windows.Settings
{
    public class InputAssignmentWindow : WindowBase, IUIStackable
    {
        public static InputAssignmentWindow Instance { get; private set; }
        
        private Action<string> _onInputAssigned;

        private void Awake()
        {
            Instance = this;
        }

        public void OpenForAssignment(Action<string> callback)
        {
            _onInputAssigned = callback;
            BeginWaitingForInput();
        }


        private void BeginWaitingForInput()
        {
            InputManager.Instance.WaitForInput(OnInputReceived);
        }


        private void OnInputReceived(string input)
        {
            var callback = _onInputAssigned;

            _onInputAssigned = null;

            callback?.Invoke(input);
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
            UIManager.BackgroundDim.Activate(false);
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
                InputManager.Instance.StopWaitingForInput();
                Deactivate(instant, onDeactivate);
                return;
            }
            Deactivate(true, onDeactivate);
            UIManager.BackgroundDim.Deactivate(false);
        }

        public void OnPopOther()
        {
            EnableInteractions();
        }

        #endregion
    }
}