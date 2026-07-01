namespace UserInterface.Screen
{
    public abstract class ScreenBase : UIComponentBase, IScreen
    {
        public virtual NamedScreen Name { get; }
        public virtual UIPriority Priority { get; }
        
    }
}