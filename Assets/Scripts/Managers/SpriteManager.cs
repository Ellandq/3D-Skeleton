using UnityEngine;
using UserInterface.Screen.Components.Settings.UserInterface.Screen.Components.Settings;

namespace Managers
{
    public class SpriteManager : ManagerBase<SpriteManager>
    {
        [Header("Input Key Dictionary")]
        [SerializeField] private InputKeySpriteDictionary inputKeySpriteDictionary;
    }
}