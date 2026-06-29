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
        [SerializeField] private UIComponentState state;
        
        [Header("Object References")]
        [SerializeField] private Button button;
        [SerializeField] private Image background;
        [SerializeField] private Image frame;
        [SerializeField] private Image icon;

        private void Awake()
        { 
            button.onClick.AddListener(() => UIManager.Instance.ActivateComponent(NamedWindow.InputAssignment));
        }

        public void ChangeState(UIComponentState state)
        {
            if (state == this.state)
                return;
            this.state = state;
            var colors = UITheme.GetColors(state);
            var lighterC = colors[UIColorType.Lighter];
            var darkerC = colors[UIColorType.Darker];
            
            background.color = darkerC;
            frame.color = lighterC;
            icon.color = lighterC;
        }
        
        public void UpdateIcon(Sprite sprite)
        {
            icon.sprite = sprite;
        }
    }
}