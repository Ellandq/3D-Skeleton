namespace UserInterface.Screen
{
    public interface IScreen
    {
        NamedScreen Name { get; }
        UIPriority Priority { get; }
    }
}