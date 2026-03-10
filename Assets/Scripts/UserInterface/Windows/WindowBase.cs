using UserInterface.Screen;

namespace UserInterface.Windows
{
    public abstract class WindowBase : UIComponentBase, IWindow
    {
        public NamedWindow Name { get; }
        public UIPriority Priority { get; }
    }
}