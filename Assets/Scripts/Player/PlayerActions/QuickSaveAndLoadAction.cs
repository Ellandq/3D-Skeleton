using Managers;
using Model.Enum.GameInput;

namespace Player.PlayerActions
{
    public class QuickSaveAndLoadAction : IPlayerAction
    {
        private PlayerController playerController;
        
        public void Initialize(PlayerController controller)
        {
            playerController = controller;

            var im = InputManager.Instance;
            
            im.Subscribe(PlayerAction.QuickLoad, QuickLoad);
            im.Subscribe(PlayerAction.QuickSave, QuickSave);
        }

        private void QuickSave(ButtonState state)
        {
            
        }

        private void QuickLoad(ButtonState state)
        {
            
        }
    }
}