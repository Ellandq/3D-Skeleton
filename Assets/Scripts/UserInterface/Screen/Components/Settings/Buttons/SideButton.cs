using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UserInterface.Components;
using UserInterface.Screen.Components.Utils;
using Utils.Enum.UI;
using Utils.SO;

namespace UserInterface.Screen.Components.Settings.Buttons
{
    [RequireComponent(typeof(UIPointerObserver))]
    public class SideButton : UISelectable
    {
        [Header("Object References")]
        [SerializeField] private TMP_Text text;
        [SerializeField] private Image background;
        
        public override void ChangeState(UIComponentState newState)
        {
            base.ChangeState(newState);
            text.color = UITheme.GetColor(state, UIColorType.Lighter);
            background.color = UITheme.GetColor(state, UIColorType.Light);
        }
    }
}