using UserInterface.Screen;

namespace UserInterface.Windows
{
    public abstract class WindowBase : UIComponentBase, IWindow
    {
        public virtual NamedWindow Name { get; }
        public virtual UIPriority Priority { get; }
    }
}