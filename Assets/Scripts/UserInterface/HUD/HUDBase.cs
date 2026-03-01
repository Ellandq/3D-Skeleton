namespace UserInterface.HUD
{
    public abstract class HUDBase : UIComponentBase, IHUD
    {
        public virtual NamedHUD Name { get; }
    }
}