using Managers;
using Model.Enum.GameInput;
using Model.Enum.Misc;
using Model.Enum.Named;
using UnityEngine;

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
            if (state == ButtonState.Up)
                return;
            Debug.Log("QUICK SAVE");
            _ = SaveManager.Instance.SaveGameAsync(SaveType.QuickSave);
        }

        private void QuickLoad(ButtonState state)
        {
            if (state == ButtonState.Up)
                return;
            Debug.Log("QUICK LOAD");
            SaveManager.SetSelectedSave(SaveManager.GetMostRecentSaveName());
            GameManager.PushState(NamedState.GameLoad);
        }
    }
}