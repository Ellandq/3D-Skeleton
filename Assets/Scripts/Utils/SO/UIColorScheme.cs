using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.Enum;

namespace Utils.SO
{
    [CreateAssetMenu(menuName = "UI/Color Scheme")]
    public class UIColorScheme : ScriptableObject
    {
        public List<StateColors> states;

        [System.Serializable]
        public class StateColors
        {
            public UIComponentState state;
            public List<TypeColor> colors;
        }

        [System.Serializable]
        public class TypeColor
        {
            public UIColorType type;
            public Color color;
        }
        
        
        public Dictionary<UIColorType, Color> GetColors(UIComponentState state)
        {
            return states
                .Where(s => s.state == state)
                .SelectMany(s => s.colors)
                .ToDictionary(c => c.type, c => c.color);
        }

        public Color GetColor(UIComponentState state, UIColorType type)
        {
            var stateEntry = states.Find(s => s.state == state);
            var colorEntry = stateEntry?.colors.Find(c => c.type == type);
            return colorEntry?.color ?? Color.magenta;
        }
    }
    
    public static class UITheme
    {
        private static UIColorScheme _scheme;

        private static UIColorScheme Scheme
        {
            get
            {
                if (!_scheme)
                {
                    _scheme = Resources.Load<UIColorScheme>("UIColorScheme");
                }
                return _scheme;
            }
        }
        
        public static Dictionary<UIColorType, Color> GetColors(UIComponentState state)
        {
            return Scheme.GetColors(state);
        }

        public static Color GetColor(UIComponentState state, UIColorType type)
        {
            return Scheme.GetColor(state, type);
        }
    }
}