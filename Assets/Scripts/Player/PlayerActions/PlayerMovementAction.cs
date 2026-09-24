namespace Player.PlayerActions
{
    public class PlayerMovementAction : IPlayerAction
    {
        private PlayerController playerController;
        
        public void Initialize(PlayerController controller)
        {
            playerController = controller;
        }
    }
}