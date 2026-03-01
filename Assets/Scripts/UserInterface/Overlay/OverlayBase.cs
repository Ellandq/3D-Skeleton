namespace UserInterface.Overlay
{
    public abstract class OverlayBase : UIComponentBase, IOverlay
    {
        public virtual NamedOverlay Name { get; }
    }
}