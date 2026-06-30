using UnityEngine;
using UserInterface.Screen.Components.Settings.UserInterface.Screen.Components.Settings;

namespace Managers
{
    public class SpriteManager : ManagerBase<SpriteManager>
    {
        [Header("Input Keys")]
        [SerializeField] private InputKeySpriteDictionary inputKeySpriteDictionary;
        [SerializeField] private Sprite defaultSprite;

        public static Sprite GetInputSprite(string key)
        {
            return Instance.inputKeySpriteDictionary.TryGetValue(key, out var sprite) ? sprite : Instance.defaultSprite;
        }
    }
}