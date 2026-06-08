using TMPro;
using UnityEngine;

namespace UserInterface.Screen.Components.Settings
{
    public class SettingsCategoryDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        
        public void Initialize(string categoryName)
        {
            text.text = categoryName;
        }
    }
}