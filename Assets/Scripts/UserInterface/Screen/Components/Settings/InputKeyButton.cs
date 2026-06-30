using Managers;
using UnityEngine;
using UnityEngine.UI;
using UserInterface.Windows;
using Utils.Enum;
using Utils.SO;

namespace UserInterface.Screen.Components.Settings
{
    public class InputKeyButton : MonoBehaviour
    {
        [SerializeField] private Button button;

        [Header("Visuals")]
        [SerializeField] private Image background;
        [SerializeField] private Image frame;
        [SerializeField] private Image icon;

        [Header("Runtime")]
        private UIComponentState state;
        private InputKeySetting _parent;
        private bool _isAlt;

        public void Initialize(InputKeySetting parent, bool isAlt = false)
        {
            _parent = parent;
            _isAlt = isAlt;
            
            button.onClick.AddListener(
                parent.SelectItem);
            
            button.onClick.AddListener(
                () => UIManager.Instance.ActivateComponent(NamedWindow.InputAssignment, false, StartAssignment)
            );
        }

        private void StartAssignment()
        {
            InputAssignmentWindow.Instance.OpenForAssignment(input =>
            {
                _parent.SettingChanged(input, _isAlt);
            });
        }


        public void ChangeState(UIComponentState newState)
        {
            state = newState;

            var colors =
                UITheme.GetColors(state);

            background.color =
                colors[UIColorType.Darker];

            frame.color =
                colors[UIColorType.Lighter];

            icon.color =
                colors[UIColorType.Lighter];

            button.interactable =
                state != UIComponentState.Disabled;
        }
        
        


        public void UpdateIcon(Sprite sprite)
        {
            icon.sprite = sprite;
        }
    }
}