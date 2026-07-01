using UnityEngine;
using UnityEngine.UI;
using UserInterface.Components;
using UserInterface.Screen.Components.Utils;
using Utils.Enum.UI;
using Utils.SO;

namespace UserInterface.Screen.Components.MainMenu
{
    [RequireComponent(typeof(UIPointerObserver))]
    public class MainMenuButton : UISelectable
    {
        [Header("Object References")] 
        [SerializeField] private Image frame;
        [SerializeField] private Image frameOffset;
        [SerializeField] private Image background;

        public override void ChangeState(UIComponentState newState)
        {
            base.ChangeState(newState);
            frame.color = UITheme.GetColor(state, UIColorType.Lighter);
            frameOffset.color = UITheme.GetColor(state, UIColorType.Darker);
            var a = background.color.a;

            var color = UITheme.GetColor(state, UIColorType.Darker);
            color.a = a;

            background.color = color;
        }
    }
}