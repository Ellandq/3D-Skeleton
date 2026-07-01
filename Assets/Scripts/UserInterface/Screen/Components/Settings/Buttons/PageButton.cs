using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.Enum.UI;
using Utils.SO;

namespace UserInterface.Screen.Components.Settings.Buttons
{
    public class PageButton : MonoBehaviour
    {
        [Header("Object References")] 
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text text;
        [SerializeField] private Image background;

        public void Initialize(Action<int> initializePageCallback, int index, string pageName)
        {
            text.text = pageName;
            ChangeState(UIComponentState.Enabled);

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => initializePageCallback(index));
        }
        
        public void ChangeState(UIComponentState newState)
        {
            var colors = UITheme.GetColors(newState);

            var lighter = colors[UIColorType.Lighter];
            var darker = colors[UIColorType.Light];

            text.color = lighter;
            background.color = darker;
        }
    }
}