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

        private UIComponentState state;


        private void Awake()
        {
            button.onClick.AddListener(
                () =>
                    UIManager.Instance.ActivateComponent(
                        NamedWindow.InputAssignment));
        }


        public void Initialize(InputKeySetting parent)
        {
            button.onClick.AddListener(
                parent.SelectItem);
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