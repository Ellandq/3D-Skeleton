namespace UserInterface.Windows
{
    public interface IWindow
    {
        NamedWindow Name { get; }
        UIPriority Priority { get; }
    }
}