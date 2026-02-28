using UnityEngine;

namespace UserInterface.Screen
{
    public abstract class ScreenBase : UIComponentBase, IScreen
    {
        public virtual NamedScreen Name { get; }
        
    }
}